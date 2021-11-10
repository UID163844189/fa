﻿using fac.ASTs.Exprs;
using fac.ASTs.Stmts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace fac {
	static class StaticHelper {
		public static string GetDisplayName (this Enum _enum) {
			var _def_name = $"{_enum}";
			var _info = _enum.GetType ().GetField (_def_name);
			var _attrs = _info.GetCustomAttributes (typeof (DisplayAttribute), false) as DisplayAttribute [];
			return _attrs.Length > 0 ? _attrs [0].Name : _def_name;
		}

		public static string Indent (this int _indent) => new string (' ', _indent * 4);

		public static void Traversal (this List<IAstStmt> _stmts, int _deep, int _group, Func<IAstExpr, int, int, IAstExpr> _cb) {
			for (int i = 0; i < _stmts.Count; ++i)
				_stmts[i] = _cb (_stmts[i], _deep, _group) as IAstStmt;
		}

		public static void TraversalCalcType (this List<IAstStmt> _stmts) {
			for (int i = 0; i < _stmts.Count; ++i)
				_stmts[i] = _stmts[i].TraversalCalcType ("") as IAstStmt;
		}

		public static void AppendStmts (this StringBuilder _sb, List<IAstStmt> _stmts, int _indent) {
			foreach (var _code in _stmts) {
				var (_a, _b) = _code.GenerateCSharp (_indent);
				_sb.Append (_a).Append (_b);
			}
		}
	}
}
