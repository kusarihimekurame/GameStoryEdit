using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GameStoryEdit.TreeData
{
    public class SolutionViewModel
    {
        #region Data

        readonly ReadOnlyCollection<TreeViewItemViewModel> _firstGeneration;
        readonly Solution _solution;
        readonly TreeViewItemViewModel[] _projects;
        readonly ICommand _searchCommand;

        IEnumerator<TreeViewItemViewModel> _matchingTreeItemEnumerator;
        string _searchText = string.Empty;

        #endregion // Data

        #region Constructor

        public SolutionViewModel(Solution solution)
        {
            _solution = solution;

            _projects = solution.Projects.Select(p => new TreeViewItemViewModel(p)).ToArray();

            _firstGeneration = new ReadOnlyCollection<TreeViewItemViewModel>(_projects);

            _searchCommand = new SearchSolutionTreeCommand(this);
        }

        #endregion // Constructor

        #region Properties

        #region FirstGeneration

        /// <summary>
        /// Returns a read-only collection containing the first treeItem 
        /// in the solution tree, to which the TreeView can bind.
        /// </summary>
        public ReadOnlyCollection<TreeViewItemViewModel> FirstGeneration => _firstGeneration;

        #endregion // FirstGeneration

        #region Solution

        public Solution Solution => _solution;

        #endregion // Solution

        #region SearchCommand

        /// <summary>
        /// Returns the command used to execute a search in the family tree.
        /// </summary>
        public ICommand SearchCommand => _searchCommand;

        private class SearchSolutionTreeCommand : ICommand
        {
            readonly SolutionViewModel _SolutionTree;

            public SearchSolutionTreeCommand(SolutionViewModel solutionTree)
            {
                _SolutionTree = solutionTree;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            event EventHandler ICommand.CanExecuteChanged
            {
                // I intentionally left these empty because
                // this command never raises the event, and
                // not using the WeakEvent pattern here can
                // cause memory leaks.  WeakEvent pattern is
                // not simple to implement, so why bother.
                add { }
                remove { }
            }

            public void Execute(object parameter)
            {
                _SolutionTree.PerformSearch();
            }
        }

        #endregion // SearchCommand

        #region SearchText

        /// <summary>
        /// Gets/sets a fragment of the name to search for.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (value == _searchText)
                    return;

                _searchText = value;

                _matchingTreeItemEnumerator = null;
            }
        }

        #endregion // SearchText

        #endregion // Properties

        #region Search Logic

        void PerformSearch()
        {
            if (_matchingTreeItemEnumerator == null || !_matchingTreeItemEnumerator.MoveNext())
                VerifyMatchingPeopleEnumerator();

            var person = _matchingTreeItemEnumerator.Current;

            if (person == null)
                return;

            // Ensure that this person is in view.
            if (person.Parent != null)
                person.Parent.IsExpanded = true;

            person.IsSelected = true;
        }

        void VerifyMatchingPeopleEnumerator()
        {
            IEnumerable<TreeViewItemViewModel> matches = FindMatches(_searchText, _projects);
            _matchingTreeItemEnumerator = matches.GetEnumerator();

            if (!_matchingTreeItemEnumerator.MoveNext())
            {
                MessageBox.Show(
                    "No matching names were found.",
                    "Try Again",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                    );
            }
        }

        IEnumerable<TreeViewItemViewModel> FindMatches(string searchText, TreeViewItemViewModel treeItem)
        {
            if (treeItem.NameContainsText(searchText))
                yield return treeItem;

            foreach (TreeViewItemViewModel child in treeItem.Children)
                foreach (TreeViewItemViewModel match in FindMatches(searchText, child))
                    yield return match;
        }
        IEnumerable<TreeViewItemViewModel> FindMatches(string searchText, TreeViewItemViewModel[] treeItems)
        {
            foreach(TreeViewItemViewModel treeItem in treeItems)
                foreach(TreeViewItemViewModel findMatch in FindMatches(searchText, treeItem))
                    yield return findMatch;
        }

        #endregion // Search Logic
    }
}
