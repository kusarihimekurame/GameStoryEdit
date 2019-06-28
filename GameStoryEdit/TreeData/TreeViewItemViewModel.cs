using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameStoryEdit.TreeData
{
    /// <summary>
    /// A UI-friendly wrapper around a Person object.
    /// </summary>
    public class TreeViewItemViewModel : INotifyPropertyChanged
    {
        #region Data

        readonly ReadOnlyCollection<TreeViewItemViewModel> _children;
        readonly TreeViewItemViewModel _parent;
        readonly ITreeItem _treeItem;

        bool _isExpanded;
        bool _isSelected;

        #endregion // Data

        #region Constructors

        public TreeViewItemViewModel(ITreeItem treeItem)
            : this(treeItem, null)
        {
        }

        private TreeViewItemViewModel(ITreeItem treeItem, TreeViewItemViewModel parent)
        {
            _treeItem = treeItem;
            _parent = parent;

            _children = new ReadOnlyCollection<TreeViewItemViewModel>(_treeItem.Children.Select(child => new TreeViewItemViewModel(child, this)).ToList());
        }

        #endregion // Constructors

        #region TreeItem Properties

        public ReadOnlyCollection<TreeViewItemViewModel> Children => _children;

        public string Name => _treeItem.Name;
        public string Path => _treeItem.Path;

        #endregion // Person Properties

        #region Presentation Members

        #region IsExpanded

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;
            }
        }

        #endregion // IsExpanded

        #region IsSelected

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion // IsSelected

        #region NameContainsText

        public bool NameContainsText(string text)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(Name))
                return false;

            return Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        #endregion // NameContainsText

        #region Parent

        public TreeViewItemViewModel Parent => _parent;

        #endregion // Parent

        #endregion // Presentation Members        

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members
    }
}
