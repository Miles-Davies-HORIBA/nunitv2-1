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
using System;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace NUnit.Util
{
	using NUnit.Core;
	using NUnit.Framework;

	/// <summary>
	/// TestSuiteTreeView is a tree view control
	/// specialized for displaying the tests
	/// in an assembly. Clients should always
	/// use TestNode rather than TreeNode when
	/// dealing with this class to be sure of
	/// calling the proper methods.
	/// </summary>
	public class TestSuiteTreeView : TreeView
	{
		#region Instance Variables

		/// <summary>
		/// Hashtable provides direct access to TestNodes
		/// </summary>
		private Hashtable treeMap = new Hashtable();
	
		/// <summary>
		/// The TestNode on which a right click was done
		/// </summary>
		private TestNode contextNode;

		#endregion

		#region Properties

		/// <summary>
		/// A type-safe version of SelectedNode.
		/// </summary>
		public new TestNode SelectedNode
		{
			get	{ return base.SelectedNode as TestNode;	}
			set	{ base.SelectedNode = value; }
		}

		/// <summary>
		/// A type-safe version of TopNode.
		/// </summary>
		public new TestNode TopNode
		{
			get	{ return base.TopNode as TestNode;	}
		}

		/// <summary>
		/// A type-safe way to get the root node
		/// of the tree. Presumed to be unique.
		/// </summary>
		public TestNode RootNode
		{
			get { return Nodes[0] as TestNode; }
		}
		
		/// <summary>
		/// The TestNode that any context menu
		/// commands will operate on.
		/// </summary>
		public TestNode ContextNode
		{
			get	{ return contextNode; }
		}

		public TestNode this[Test test]
		{
			get { return treeMap[test.FullName] as TestNode; }
		}

		/// <summary>
		/// The currently selected test suite
		/// </summary>
		public Test SelectedSuite
		{
			get 
			{ 
				if ( SelectedNode == null )
					return null;
				
				return SelectedNode.Test; 
			}
		}

		/// <summary>
		/// The test that any context menu commands
		/// will apply to.
		/// </summary>
		public Test ContextSuite
		{
			get	
			{ 
				if ( ContextNode == null )
					return null;

				return ContextNode.Test; 
			}
		}
		#endregion

		#region Methods

		/// <summary>
		/// Handles right mouse button down by
		/// remembering the proper context item.
		/// </summary>
		/// <param name="e">MouseEventArgs structure with information about the mouse position and button state</param>
		protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right )
			{
				TreeNode theNode = GetNodeAt( e.X, e.Y );
				if ( theNode != null )
					contextNode = theNode as TestNode;
			}

			base.OnMouseDown( e );
		}

		/// <summary>
		/// A type-safe version of GetNodeAt
		/// </summary>
		/// <param name="x">X Position for which the node is to be returned</param>
		/// <param name="y">Y Position for which the node is to be returned</param>
		/// <returns></returns>
		public new TestNode GetNodeAt(int x, int y)
		{
			return base.GetNodeAt(x, y) as TestNode;
		}

		/// <summary>
		/// A type-safe version of GetNodeAt
		/// </summary>
		/// <param name="pt">Position for which the node is to be returned</param>
		/// <returns></returns>
		public new TestNode GetNodeAt(Point pt)
		{
			return base.GetNodeAt(pt) as TestNode;
		}

		/// <summary>
		/// Clear all the results in the tree.
		/// </summary>
		public void ClearResults()
		{
			foreach ( TestNode rootNode in Nodes )
				rootNode.ClearResults();
		}

		/// <summary>
		/// Load the tree with a test hierarchy
		/// </summary>
		/// <param name="test">Test to be loaded</param>
		public void Load( Test test )
		{
			Clear();
			AddTreeNodes( Nodes, test, false );
			ExpandAll();
			SelectedNode = Nodes[0] as TestNode;
		}

		/// <summary>
		/// Add nodes to the tree constructed from a test
		/// </summary>
		/// <param name="nodes">The TreeNodeCollection to which the new node should  be added</param>
		/// <param name="rootTest">The test for which a node is to be built</param>
		/// <param name="highlight">If true, highlight the text for this node in the tree</param>
		/// <returns>A newly constructed TestNode, possibly with descendant nodes</returns>
		private TestNode AddTreeNodes( IList nodes, Test rootTest, bool highlight )
		{
			TestNode node = new TestNode( rootTest );
//			if ( highlight ) node.ForeColor = Color.Blue;
			treeMap.Add( node.Test.FullName, node );
			nodes.Add( node );
			
			TestSuite testSuite = rootTest as TestSuite;
			if ( testSuite != null )
			{
				foreach( Test test in testSuite.Tests )
					AddTreeNodes( node.Nodes, test, highlight );
			}

			return node;
		}

		private void RemoveFromMap( TestNode node )
		{
			foreach( TestNode child in node.Nodes )
				RemoveFromMap( child );

			treeMap.Remove( node.Test.FullName );
		}

		/// <summary>
		/// Remove a node from the tree itself and the hashtable
		/// </summary>
		/// <param name="node">Node to remove</param>
		public void RemoveNode( TestNode node )
		{
			if ( contextNode == node )
				contextNode = null;
			RemoveFromMap( node );
			node.Remove();
		}

		/// <summary>
		/// Reload the tree with a changed test hierarchy
		/// while maintaining as much gui state as possible
		/// </summary>
		/// <param name="test">Test suite to be loaded</param>
		public void Reload( Test test )
		{
			if ( !Match( RootNode, test ) )
				throw( new ArgumentException( "Reload called with non-matching test" ) );
				
			UpdateNode( RootNode, test );
		}

		/// <summary>
		/// Helper routine that compares a node with a test
		/// </summary>
		/// <param name="node">Node to compare</param>
		/// <param name="test">Test to compare</param>
		/// <returns>True if the test has the same name</returns>
		private bool Match( TestNode node, Test test )
		{
			return node.Test.FullName == test.FullName;
		}

		/// <summary>
		/// A node has been matched with a test, so update it
		/// and then process child nodes and tests recursively.
		/// If a child was added or removed, then this node
		/// will expand itself.
		/// </summary>
		/// <param name="node">Node to be updated</param>
		/// <param name="test">Test to plug into node</param>
		/// <returns>True if a child node was added or deleted</returns>
		private bool UpdateNode( TestNode node, Test test )
		{
			node.UpdateTest( test );
			
			TestSuite suite = test as TestSuite;
			if ( suite == null )
				return false;

			bool showChildren = UpdateNodes( node.Nodes, suite.Tests );

			if ( showChildren ) node.Expand();

			return showChildren;
		}

		/// <summary>
		/// Match a set of nodes against a set of tests.
		/// Remove nodes that are no longer represented
		/// in the tests. Update any nodes that match.
		/// Add new nodes for new tests.
		/// </summary>
		/// <param name="nodes">List of nodes to be matched</param>
		/// <param name="tests">List of tests to be matched</param>
		/// <returns>True if the parent should expand to show that something was added or deleted</returns>
		private bool UpdateNodes( IList nodes, IList tests )
		{
			bool showChanges = false;

			foreach( TestNode node in nodes )
				if ( NodeWasDeleted( node, tests ) )
				{
					RemoveNode( node );
					showChanges = true;
				}

			foreach( Test test in tests )
			{
				TestNode node = this[ test ];
				if ( node == null )
				{
					AddTreeNodes( nodes, test, true );
					showChanges = true;
				}
				else
					UpdateNode( node, test );
			}

			return showChanges;
		}

		/// <summary>
		/// Helper returns true if the node test is not in
		/// the list of tests provided.
		/// </summary>
		/// <param name="node">Node to examine</param>
		/// <param name="tests">List of tests to match with node</param>
		private bool NodeWasDeleted( TestNode node, IList tests )
		{
			foreach ( Test test in tests )
				if( Match( node, test ) )
					return false;

			return true;
		}

		/// <summary>
		/// Delegate for use in invoking the tree loader
		/// from the watcher thread.
		/// </summary>
		private delegate void LoadHandler( Test test );
		
		/// <summary>
		/// Called to load the tree from the watcher thread.
		/// </summary>
		/// <param name="test"></param>
		public void InvokeLoadHandler( Test test )
		{
			Invoke( new LoadHandler( Reload ), new object[]{ test } );
		}

		/// <summary>
		/// Clear all the info in the tree.
		/// </summary>
		public void Clear()
		{
			treeMap.Clear();
			Nodes.Clear();
		}

		/// <summary>
		/// Add the result of a test to the tree
		/// </summary>
		/// <param name="result">The result of the test</param>
		public void SetTestResult(TestResult result)
		{
			TestNode node = this[result.Test];	
			if ( node != null )
				node.SetResult( result );
			else
				Console.Error.WriteLine("Could not locate node: " + result.Test.FullName + " in tree map");
		}

		/// <summary>
		/// Find and expand a particular test in the tree
		/// </summary>
		/// <param name="test">The test to expand</param>
		public void Expand( Test test )
		{
			TestNode node = this[test];
			if ( node != null )
				node.Expand();
		}

#if CHARLIE		
		/// <summary>
		/// Helper figures out if a node represents a test fixture
		/// </summary>
		/// <param name="node">Node to be examined</param>
		/// <returns>True if the node represents a test fixture</returns>
		private bool NodeIsFixture( TestNode node )
		{
			// Test case isn't a fixture
			if ( node.Test is NUnit.Core.TestSuite )
				return false;
			
			// Suite with no children can only be a fixture
			if ( node.Nodes.Count == 0 )
				return true;

			// Otherwise, it depends on what kind of children it has
			TestNode child = (TestNode)node.Nodes[0];
			return child.Test is NUnit.Core.TestCase;
		}

		/// <summary>
        /// Collapse all fixtures in the tree
        /// </summary>
		public void CollapseFixtures()
		{
			CollapseFixtures( RootNode );
		}

		/// <summary>
		/// Helper collapses all fixtures under a node
		/// </summary>
		/// <param name="node">Node under which to collapse fixtures</param>
		private void CollapseFixtures( TestNode node )
		{
			if ( NodeIsFixture( node ) )
				node.Collapse();
			else 
				foreach( TestNode child in node.Nodes )
					CollapseFixtures( child );		
		}

		/// <summary>
		/// Expand all fixtures in the tree
		/// </summary>
		public void ExpandFixtures()
		{
			ExpandFixtures( RootNode );
		}

		/// <summary>
		/// Helper expands all fixtures under a node
		/// </summary>
		/// <param name="node">Node under which to expand fixtures</param>
		private void ExpandFixtures( TestNode node )
		{
			if ( NodeIsFixture( node ) )
				node.Expand();
			else 
				foreach( TestNode child in node.Nodes )
					ExpandFixtures( child );		
		}
#endif
        #endregion
	}
}

