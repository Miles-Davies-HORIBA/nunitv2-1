/********************************************************************************************************************
'
' Copyright (c) 2002, James Newkirk, Michael C. Two, Alexei Vorontsov, Philip Craig
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
' THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
'
'*******************************************************************************************************************/
namespace NUnit.Tests
{
	using System;
	using System.Collections;
	using Microsoft.Win32;

	using NUnit.Framework;
	using NUnit.Util;

	/// <summary>
	/// Summary description for RecentAssemblyFixture.
	/// </summary>
	/// 
	[TestFixture]
	public class RecentAssemblyFixture
	{
		RecentAssemblyUtil assemblies;

		[SetUp]
		public void CreateUtil()
		{
			assemblies = new RecentAssemblyUtil("test-recent-assemblies");
		}

		[TearDown]
		public void ClearRegistry()
		{
			assemblies.Clear();
		}

		[Test]
		public void RetrieveSubKey()
		{
			Assertion.AssertNotNull(assemblies);
		}

		[Test]
		public void GetMostRecentAssembly()
		{
			string assemblyFileName = "tests.dll";
			Assertion.AssertNull("first time this should be null", assemblies.RecentAssembly);
			assemblies.RecentAssembly = assemblyFileName;
			Assertion.AssertEquals(assemblyFileName, assemblies.RecentAssembly);
		}

		[Test]
		public void GetAssemblies()
		{
			assemblies.RecentAssembly = "3";
			assemblies.RecentAssembly = "2";
			assemblies.RecentAssembly = "1";
			IList list = assemblies.GetAssemblies();
			Assertion.AssertEquals(3, list.Count);
			Assertion.AssertEquals("1", list[0]);
		}


		private void SetMockRegistryValues()
		{
			assemblies.RecentAssembly = "5";
			assemblies.RecentAssembly = "4";
			assemblies.RecentAssembly = "3";
			assemblies.RecentAssembly = "2";
			assemblies.RecentAssembly = "1";  // this is the most recent
		}

		[Test]
		public void ReorderAssemblies5()
		{
			SetMockRegistryValues();
			assemblies.RecentAssembly = "5";

			IList assemblyList = assemblies.GetAssemblies();
			Assertion.AssertEquals("5", assemblyList[0]);
			Assertion.AssertEquals("1", assemblyList[1]);
			Assertion.AssertEquals("2", assemblyList[2]);
			Assertion.AssertEquals("3", assemblyList[3]);
			Assertion.AssertEquals("4", assemblyList[4]);
		}

		[Test]
		public void ReorderAssemblies4()
		{
			SetMockRegistryValues();
			assemblies.RecentAssembly = "4";

			IList assemblyList = assemblies.GetAssemblies();
			Assertion.AssertEquals("4", assemblyList[0]);
			Assertion.AssertEquals("1", assemblyList[1]);
			Assertion.AssertEquals("2", assemblyList[2]);
			Assertion.AssertEquals("3", assemblyList[3]);
			Assertion.AssertEquals("5", assemblyList[4]);
		}

		[Test]
		public void ReorderAssembliesNew()
		{
			SetMockRegistryValues();
			assemblies.RecentAssembly = "6";

			IList assemblyList = assemblies.GetAssemblies();
			Assertion.AssertEquals("6", assemblyList[0]);
			Assertion.AssertEquals("1", assemblyList[1]);
			Assertion.AssertEquals("2", assemblyList[2]);
			Assertion.AssertEquals("3", assemblyList[3]);
			Assertion.AssertEquals("4", assemblyList[4]);
		}


		[Test]
		public void ReorderAssemblies3()
		{
			SetMockRegistryValues();
			assemblies.RecentAssembly = "3";

			IList assemblyList = assemblies.GetAssemblies();
			Assertion.AssertEquals("3", assemblyList[0]);
			Assertion.AssertEquals("1", assemblyList[1]);
			Assertion.AssertEquals("2", assemblyList[2]);
			Assertion.AssertEquals("4", assemblyList[3]);
			Assertion.AssertEquals("5", assemblyList[4]);
		}

		[Test]
		public void ReorderAssemblies2()
		{
			SetMockRegistryValues();
			assemblies.RecentAssembly = "2";

			IList assemblyList = assemblies.GetAssemblies();
			Assertion.AssertEquals("2", assemblyList[0]);
			Assertion.AssertEquals("1", assemblyList[1]);
			Assertion.AssertEquals("3", assemblyList[2]);
			Assertion.AssertEquals("4", assemblyList[3]);
			Assertion.AssertEquals("5", assemblyList[4]);
		}

		[Test]
		public void ReorderAssemblies1()
		{
			SetMockRegistryValues();
			assemblies.RecentAssembly = "1";

			IList assemblyList = assemblies.GetAssemblies();
			Assertion.AssertEquals("1", assemblyList[0]);
			Assertion.AssertEquals("2", assemblyList[1]);
			Assertion.AssertEquals("3", assemblyList[2]);
			Assertion.AssertEquals("4", assemblyList[3]);
			Assertion.AssertEquals("5", assemblyList[4]);
		}

		[Test]
		public void AddAssemblyListNotFull()
		{
			assemblies.RecentAssembly = "3";
			assemblies.RecentAssembly = "2";
			assemblies.RecentAssembly = "1";  // this is the most recent

			assemblies.RecentAssembly = "3";

			IList assemblyList = assemblies.GetAssemblies();
			Assertion.AssertEquals(3, assemblyList.Count);
			Assertion.AssertEquals("3", assemblyList[0]);
			Assertion.AssertEquals("1", assemblyList[1]);
			Assertion.AssertEquals("2", assemblyList[2]);
		}

		[Test]
		public void AddAssemblyToList()
		{
			assemblies.RecentAssembly = "1";
			assemblies.RecentAssembly = "3";

			IList assemblyList = assemblies.GetAssemblies();
			Assertion.AssertEquals(2, assemblyList.Count);
			Assertion.AssertEquals("3", assemblyList[0]);
			Assertion.AssertEquals("1", assemblyList[1]);
		}

		[Test]
		public void RemoveAssemblyFromList()
		{
			assemblies.RecentAssembly = "3";
			assemblies.RecentAssembly = "2";
			assemblies.RecentAssembly = "1";

			assemblies.Remove("2");

			IList assemblyList = assemblies.GetAssemblies();
			Assertion.AssertEquals(2, assemblyList.Count);
			Assertion.AssertEquals("1", assemblyList[0]);
			Assertion.AssertEquals("3", assemblyList[1]);
		}
	}
}
