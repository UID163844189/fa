﻿using fac.AntlrTools;
using fac.ASTs.Types;
using fac.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Exprs {
	class AstExpr_Array: IAstExpr {
		public IAstType ItemDataType { get; set; }
		public IAstExpr InitCount { get; set; }
		public List<IAstExpr> InitValues { get; set; }



		public override void Traversal (int _deep, int _group, Func<IAstExpr, int, int, IAstExpr> _cb) {
			InitCount = _cb (InitCount, _deep, _group);
			for (int i = 0; i < InitValues.Count; ++i)
				InitValues[i] = _cb (InitValues[i], _deep, _group);
		}

		public override IAstExpr TraversalCalcType (IAstType _expect_type) {
			if (_expect_type == null) {
				if (ItemDataType == null)
					ItemDataType = (GuessType () as AstType_ArrayWrap).ItemType;
			} else if (_expect_type is AstType_ArrayWrap _tmptype) {
				ItemDataType = _tmptype.ItemType;
			} else {
				if (ItemDataType == null)
					ItemDataType = (GuessType () as AstType_ArrayWrap).ItemType;
			}

			// 处理类型
			InitCount = InitCount.TraversalCalcType (IAstType.FromName ("int"));
			for (int i = 0; i < InitValues.Count; ++i)
				InitValues[i] = InitValues[i].TraversalCalcType (ItemDataType);
			ExpectType = new AstType_ArrayWrap { Token = Token, TypeStr = $"{ItemDataType}[]", ItemType = ItemDataType };
			return AstExprTypeCast.Make (this, _expect_type);
		}

		public override IAstType GuessType () {
			if (ItemDataType != null)
				return new AstType_ArrayWrap { Token = Token, TypeStr = $"{ItemDataType}[]", ItemType = ItemDataType };
			var _item_type = TypeFuncs.GetCompatibleType ((from p in InitValues select p.GuessType ()).ToArray ());
			return new AstType_ArrayWrap { Token = Token, TypeStr = $"{_item_type.TypeStr}[]", ItemType = _item_type };
		}

		public override (string, string) GenerateCSharp (int _indent) {
			var _psb = new StringBuilder ();
			var _tmp_var_name = Common.GetTempId ();
			_psb.AppendLine ($"{_indent.Indent ()}var {_tmp_var_name} = new List<{ItemDataType}> ();");
			foreach (var _init_val in InitValues) {
				var (_a, _b) = _init_val.GenerateCSharp (_indent);
				_psb.Append (_a).AppendLine ($"{_indent.Indent ()}{_tmp_var_name}.Add ({_b});");
			}
			return (_psb.ToString (), _tmp_var_name);
		}

		public override bool AllowAssign () => false;
	}
}
