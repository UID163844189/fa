﻿using fac.AntlrTools;
using fac.ASTs.Exprs.Names;
using fac.ASTs.Types;
using fac.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Exprs {
	public class AstExpr_Op2: IAstExpr {
		public IAstExpr Value1 { get; set; }
		public IAstExpr Value2 { get; set; }
		public string Operator { get; set; }



		public override void Traversal (int _deep, int _group, Func<IAstExpr, int, int, IAstExpr> _cb) {
			Value1 = _cb (Value1, _deep, _group);
			Value2 = _cb (Value2, _deep, _group);
		}

		public override IAstExpr TraversalCalcType (IAstType _expect_type) {
			if (sCompareOp2s.Contains (Operator)) {
				// > >=
				var _exp_type = TypeFuncs.GetCompatibleType (false, Value1.GuessType (), Value2.GuessType ());
				Value1 = Value1.TraversalCalcType (_exp_type);
				Value2 = Value2.TraversalCalcType (_exp_type);
				ExpectType = IAstType.FromName ("bool");
				return AstExprTypeCast.Make (this, _expect_type);
			} else if (sLogicOp2s.Contains (Operator)) {
				// && ||
				ExpectType = IAstType.FromName ("bool");
				Value1 = Value1.TraversalCalcType (ExpectType);
				Value2 = Value2.TraversalCalcType (ExpectType);
				return AstExprTypeCast.Make (this, _expect_type);
			} else if (sNumOp2s.Contains (Operator)) {
				// + - * /
				var _exp_type = _expect_type ?? TypeFuncs.GetCompatibleType (false, Value1.GuessType (), Value2.GuessType ());
				if (_exp_type is AstType_OptionalWrap _owrap)
					_exp_type = _owrap.ItemType;
				Value1 = Value1.TraversalCalcType (_exp_type);
				Value2 = Value2.TraversalCalcType (_exp_type);
				if (Operator != "/") {
					ExpectType = _exp_type;
				} else {
					ExpectType = new AstType_OptionalWrap { Token = Token, ItemType = _exp_type };
				}
				return AstExprTypeCast.Make (this, _expect_type);
			} else if (sAssignOp2s.Contains (Operator)) {
				// = += -=
				Value1 = Value1.TraversalCalcType (null);
				Value2 = Value2.TraversalCalcType (Value1.ExpectType);
				ExpectType = Value1.ExpectType;
				return AstExprTypeCast.Make (this, _expect_type);
			} else if (sQusQusOp2s.Contains (Operator)) {
				// ?? ??=
				if (Operator == "??") {
					Value2 = Value2.TraversalCalcType (null);
					var _exp_type = _expect_type ?? Value2.ExpectType;
					_exp_type = _exp_type is AstType_OptionalWrap ? _exp_type : new AstType_OptionalWrap { Token = Token, ItemType = _exp_type };
					Value1 = Value1.TraversalCalcType (_exp_type);
					ExpectType = Value2.ExpectType;
					return AstExprTypeCast.Make (this, _expect_type);
				} else if (Operator == "??=") {
					Value1 = Value1.TraversalCalcType (null);
					if (Value1.ExpectType is AstType_OptionalWrap _owrap) {
						try {
							Value2 = Value2.TraversalCalcType (_owrap.ItemType);
							ExpectType = _owrap.ItemType;
						} catch (Exception) {
							Value2 = Value2.TraversalCalcType (Value1.ExpectType);
							ExpectType = Value1.ExpectType;
						}
						return AstExprTypeCast.Make (this, _expect_type);
					} else {
						throw new CodeException (Token, "??= 运算符左操作数必须为可空类型");
					}
				}
			}
			throw new UnimplException (Token);
		}

		public override IAstType GuessType () {
			if (sCompareOp2s.Contains (Operator) || sLogicOp2s.Contains (Operator)) {
				return IAstType.FromName ("bool");
			} else if (sNumOp2s.Contains (Operator) || sAssignOp2s.Contains (Operator)) {
				bool _opt = Operator == "/" && Info.CurrentFunc.ReturnType is not AstType_OptionalWrap;
				var _type = TypeFuncs.GetCompatibleType (true, Value1.GuessType (), Value2.GuessType ());
				if (_opt)
					_type = _type is AstType_OptionalWrap ? _type : new AstType_OptionalWrap { Token = Token, ItemType = _type };
				return _type;
				// 除法，返回不为空，那么猜测类型为    可空
				// 除法，返回为空，那么猜测类型为    不可空
				// 非除法，返回不为空，那么猜测类型为    不可空
				// 非除法，返回为空，那么猜测类型为    不可空
			} else if (sQusQusOp2s.Contains (Operator)) {
				return Value2.GuessType ();
			} else {
				throw new UnimplException (Token);
			}
		}

		public override (string, string, string) GenerateCSharp (int _indent, Action<string, string> _check_cb) {
			if (Operator == "=" && Value1 is AstExprName_Ignore)
				return Value2.GenerateCSharp (_indent, _check_cb);
			string _oper = Operator != "??" ? Operator : "|";
			var (_a, _b, _c) = Value1.GenerateCSharp (_indent, _check_cb);
			var (_d, _e, _f) = Value2.GenerateCSharp (_indent, _check_cb);
			if (Operator == "/") {
				_check_cb ($"{_e} == 0", "\"除数不能为0\"");
				return ($"{_a}{_d}", $"{ExpectType.GenerateCSharp_Type ()}.FromValue ({_b} {_oper} {_e})", $"{_c}{_f}");
			}
			if (Operator == "=") {
				if (Value1 is not IAstExprName)
					throw new CodeException (Value1.Token, "赋值运算符左侧必须为可赋值的变量或参数名称");
				//if (Value2 is AstExpr_Op1 _op1expr && (!_op1expr.IsPrefix) && _op1expr.Operator == "?") {
				//	var _ec = new ExprChecker (Value1 as IAstExprName);
				//	var (_a, _b, _c) = Value1.GenerateCSharp (_indent, null);
				//	var (_d, _e, _f) = _op1expr.Value.GenerateCSharp (_indent, _ec.CheckFunc);
				//	var _tmp_var_name = Common.GetTempId ();
				//	StringBuilder _psb = new StringBuilder ();
				//	_psb.Append (_a).Append (_d).AppendLine ($"{_indent.Indent ()}{_op1expr.Value.ExpectType.GenerateCSharp_Type ()} {_tmp_var_name} = {_e};");
				//	_ec.CheckFunc ($"!{_tmp_var_name}.HasValue ()", $"{_tmp_var_name}.GetError ()");
				//	var (_g, _h) = _ec.GenerateCSharpPrefixSuffix (_indent, Value2.Token);
				//	_psb.Append (_g);
				//	return (_psb.ToString (), $"{_b} {_oper} {_tmp_var_name}.GetValue ()", $"{_h}{_c}{_f}");
				//}

			}
			return ($"{_a}{_d}", $"{_b} {_oper} {_e}", $"{_c}{_f}");
		}

		public override bool AllowAssign () => false;

		private static HashSet<string> sCompareOp2s = new HashSet<string> { ">", "<", ">=", "<=", "==", "!=" };
		private static HashSet<string> sLogicOp2s = new HashSet<string> { "||", "&&" };
		private static HashSet<string> sNumOp2s = new HashSet<string> { "+", "-", "*", "/", "%", "|", "&", "^", "<<", ">>" };
		public static HashSet<string> sAssignOp2s = new HashSet<string> { "=", "+=", "-=", "*=", "/=", "%=", "|=", "&=", "^=", "<<=", ">>=" };
		private static HashSet<string> sQusQusOp2s = new HashSet<string> { "??", "??=" };
	}
}
