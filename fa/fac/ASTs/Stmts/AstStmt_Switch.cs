﻿using fac.AntlrTools;
using fac.ASTs.Exprs;
using fac.ASTs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Stmts {
	class AstStmt_Switch: IAstStmt {
		public IAstExpr Condition { get; set; }
		public List<IAstExpr> CaseValues { get; set; }
		public List<IAstStmt> CaseCodes { get; set; }



		public override void Traversal (int _deep, int _group, Func<IAstExpr, int, int, IAstExpr> _cb) {
			Condition = _cb (Condition, _deep, _group);
			CaseValues.Traversal (_deep + 1, 0, _cb);
			CaseCodes.Traversal (_deep + 1, 0, _cb);
		}

		public override IAstExpr TraversalCalcType (IAstType _expect_type) {
			if (_expect_type != null)
				throw new Exception ("语句类型不可指定期望类型");
			Condition = Condition.TraversalCalcType (IAstType.FromName ("bool"));
			CaseValues.TraversalCalcType ();
			CaseCodes.TraversalCalcType ();
			return this;
		}

		public override (string, string, string) GenerateCSharp (int _indent, Action<string, string> _check_cb) {
			//var _sb = new StringBuilder ();
			//var _ec = new ExprChecker (null);
			//var (_a, _b, _c) = Condition.GenerateCSharp (_indent, _ec.CheckFunc);
			//var (_d, _e) = _ec.GenerateCSharpPrefixSuffix (_indent, Condition.Token);
			//_sb.AppendLine ($"{_d}{_a}{_indent.Indent ()}if ({_b}) {{");
			//_sb.AppendStmts (IfTrueCodes, _indent + 1);
			//if (IfFalseCodes.Any ()) {
			//	_sb.AppendLine ($"{_indent.Indent ()}}} else {{");
			//	_sb.AppendStmts (IfFalseCodes, _indent + 1);
			//}
			//_sb.AppendLine ($"{_indent.Indent ()}}}");
			//return ("", _sb.ToString (), $"{_c}{_e}");
			throw new NotImplementedException ();
		}
	}
}
