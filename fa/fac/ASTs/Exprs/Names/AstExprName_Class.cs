﻿using fac.ASTs.Types;
using fac.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Exprs.Names {
	class AstExprName_Class: IAstExprName {
		public AstClass Class { init; get; }



		public override IAstExpr TraversalCalcType (IAstType _expect_type) {
			ExpectType = new AstType_Class { Token = Token, TypeStr = Class.FullName, Class = Class };
			return AstExprTypeCast.Make (this, _expect_type);
		}

		public override IAstType GuessType () => new AstType_Class { Token = Token, TypeStr = Class.FullName, Class = Class };

		public override (string, string) GenerateCSharp (int _indent) => ("", Class.FullName);

		public override bool AllowAssign () => false;
	}
}
