// ****************************************************************
// Copyright 2002-2018, Charlie Poole
// This is free software licensed under the NUnit license, a copy
// of which should be included with this software. If not, you may
// obtain a copy at https://github.com/nunit-legacy/nunitv2.
// ****************************************************************

using System;
using System.Collections;
#if CLR_2_0 || CLR_4_0
using System.Collections.Generic;
#endif

namespace NUnit.Framework.Constraints
{
    [TestFixture]
    public class XmlSerializableTest : ConstraintTestBaseWithArgumentException
    {
        [SetUp]
        public void SetUp()
        {
            theConstraint = new XmlSerializableConstraint();
            expectedDescription = "xml serializable";
            stringRepresentation = "<xmlserializable>";
        }

        internal object[] SuccessData = new object[] { 1, "a", new ArrayList() };

#if CLR_2_0 || CLR_4_0
        internal object[] FailureData = new object[] { new Dictionary<string, string>(), new InternalClass(), new InternalWithSerializableAttributeClass() };
        internal string[] ActualValues = new string[] { "<Dictionary`2>", "<InternalClass>", "<InternalWithSerializableAttributeClass>" };
#else
        internal object[] FailureData = new object[] { new InternalClass(), new InternalWithSerializableAttributeClass() };
        internal string[] ActualValues = new string[] { "<InternalClass>", "<InternalWithSerializableAttributeClass>" };
#endif


        internal object[] InvalidData = new object[] { null };

        internal class InternalClass
        { }

        [Serializable]
        internal class InternalWithSerializableAttributeClass
        { }
    }
}
