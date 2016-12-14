﻿using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Foundation.CodeGenerators.Model
{
    public class EnumType
    {
        public virtual ITypeSymbol EnumTypeSymbol { get; set; }

        public virtual IList<EnumMember> Members { get; set; }
    }

    public class EnumMember
    {
        public virtual string Name { get; set; }

        public ISymbol Symbol { get; set; }

        public virtual int Value { get; set; }
    }
}
