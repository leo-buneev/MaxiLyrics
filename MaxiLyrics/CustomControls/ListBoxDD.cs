using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;

namespace MaxiLyrics
{
    /// <summary>
    /// Provides Drag & Drop and Drag Rearrange functionality to basic WPF ListBox.
    /// </summary>
    public class ListBoxDD : ListBox
    {
        /// <summary>
        /// Provides Drag & Drop functionality to basic WPF ListBoxItem.
        /// Designed to use only with ListBoxDD control.
        /// <remarks>Class ListBoxItemDD is nested in ListBoxDD class. Therefore,
        /// to access it from XAML you should use '+', for example
        /// <code>&lt;Style TargetType="ml:ListBoxDD+ListBoxItemDD"/&gt;</code>
        /// Note, that in current version of VS2010 this breaks WPF Designer.
        /// </remarks>
        /// </summary>
        public class ListBoxItemDD : ListBoxItem
        {
            private bool mouseWasDownAndSelected = false;
            private bool needLeftButtonDownCall = false;
            /// <summary>
            /// Parent ListBoxDD.
            /// </summary>
            protected ListBoxDD parentLB;
            public ListBoxItemDD(ListBoxDD parentLB)
                : base()
            {
                this.parentLB = parentLB;
                this.BorderBrush = Brushes.Black;
            }
            #region Event Handlers
            /// <summary>
            /// Overrides standard ListBoxItem behavior. If this ListBoxItem was already selected,
            /// suppress MouseLeftButtonDown event, because it is beginning of potential drag rearrange. 
            /// </summary>
            /// <param name="e">MouseButtonEventArgs object - contains info about mouse event.</param>
            protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
            {
                if (!IsSelected)
                {
                    needLeftButtonDownCall = false;
                    mouseWasDownAndSelected = true;
                    base.OnMouseLeftButtonDown(e);
                    parentLB.ReleaseMouseCapture();
                }
                else
                {
                    e.Handled = true;
                    needLeftButtonDownCall = true;
                    mouseWasDownAndSelected = true;
                }
            }
            /// <summary>
            /// Overrides standard ListBoxItem behavior.
            /// If MouseLeftButtonDown event was suppressed, fire it.
            /// If drag rearrange operation was happening, move selected items close to this item.
            /// </summary>
            /// <param name="e">MouseButtonEventArgs object - contains info about mouse event.</param>
            protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
            {
                mouseWasDownAndSelected = false;
                if (parentLB.mouseWasDownAndSelected)
                {
                    parentLB.mouseWasDownAndSelected = false;
                    if (parentLB.AllowDragRearrange)
                    {
                        this.BorderThickness = new Thickness(0, 0, 0, 0);
                        if (e.GetPosition(this).Y < this.ActualHeight / 2)
                            parentLB.RearrangeItemsBefore(this);
                        else
                            parentLB.RearrangeItemsAfter(this);
                    }
                }
                if (needLeftButtonDownCall)
                {
                    base.OnMouseLeftButtonDown(e);
                }
                base.OnMouseLeftButtonUp(e);
            }
            /// <summary>
            /// Overrides standard ListBoxItem behaviour.
            /// If drag rearrange operation is happening, suppress MouseEnter event (otherwise it will change current selection, because left mouse button is pressed).
            /// </summary>
            /// <param name="e">MouseEventArgs object - contains info about mouse event.</param>
            protected override void OnMouseEnter(MouseEventArgs e)
            {
                if (!parentLB.mouseWasDownAndSelected)
                    base.OnMouseEnter(e);
            }
            /// <summary>
            /// Overrides standard ListBoxItem behaviour.
            /// If drag rearrange operation is happening, hide border. 
            /// </summary>
            /// <param name="e">MouseEventArgs object - contains info about mouse event.</param>
            protected override void OnMouseLeave(MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    needLeftButtonDownCall = false;
                    if (parentLB.mouseWasDownAndSelected)
                    {
                        this.BorderThickness = new Thickness(0, 0, 0, 0);
                    }
                    else if(mouseWasDownAndSelected)
                    {
                        parentLB.mouseWasDownAndSelected = true;
                    }
                    mouseWasDownAndSelected = false;
                }
                base.OnMouseLeave(e);
            }
            /// <summary>
            /// Overrides standard ListBoxItem behaviour.
            /// If drag rearrange is happening, show appropriate border side.
            /// </summary>
            /// <param name="e">MouseEventArgs object - contains info about mouse event.</param>
            protected override void OnMouseMove(MouseEventArgs e)
            {
                if (parentLB.mouseWasDownAndSelected && parentLB.AllowDragRearrange)
                {
                    if (e.GetPosition(this).Y < this.ActualHeight / 2)
                        this.BorderThickness = new Thickness(0, 1, 0, 0);
                    else
                        this.BorderThickness = new Thickness(0, 0, 0, 1);
                }
                base.OnMouseMove(e);
            }
            #endregion

        }

        private bool mouseWasDownAndSelected = false;
        public bool AllowDragRearrange { get; set; }
        public bool AllowDrag { get; set; }
        /// <summary>
        /// Registers routed event for DragBegin event.
        /// DragBegin is fired if drag is happening and mouse pointer has left this ListBox.
        /// </summary>
        public static readonly RoutedEvent DragBeginEvent = EventManager.RegisterRoutedEvent(
            "DragBegin", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ListBoxDD));
        /// <summary>
        /// Wrapper for DragBegin routed event.
        /// </summary>
        public event RoutedEventHandler DragBegin
        {
            add { AddHandler(DragBeginEvent, value); }
            remove { RemoveHandler(DragBeginEvent, value); }
        }
        void RaiseDragBeginEvent()
        {
            RoutedEventArgs e = new RoutedEventArgs(DragBeginEvent);
            RaiseEvent(e);
        }

        /// <summary>
        /// Overrides standard ListBox behaviour.
        /// While building containers for items, returns ListBoxItemDD instances instead of just ListBoxItem.
        /// </summary>
        /// <returns>ListBoxItemDD instance</returns>
        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new ListBoxItemDD(this);
        }
        #region Event Handlers
        /// <summary>
        /// Overrides standard ListBox behaviour.
        /// If drag rearrange is happening, cancel it and file DragLeave
        /// </summary>
        /// <param name="e">MouseEventArgs object - contains data about mouse event.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (mouseWasDownAndSelected && AllowDrag)
                RaiseDragBeginEvent();
            mouseWasDownAndSelected = false;
            base.OnMouseLeave(e);
        }
        #endregion
        #region Drag rearrange helper functions
        private void RearrangeItemsBefore(ListBoxItemDD lbi)
        {
            int idx = this.ItemContainerGenerator.IndexFromContainer(lbi);
            Rearrange(idx);
        }
        private void RearrangeItemsAfter(ListBoxItemDD lbi)
        {
            int idx = this.ItemContainerGenerator.IndexFromContainer(lbi);
            Rearrange(idx + 1);
        }
        private void Rearrange(int targetIdx)
        {
            List<object> selectedItems = new List<object>((IList<object>)this.SelectedItems);
            int idx;
            while ((idx = this.SelectedIndex) != -1)
            {
                this.Items.RemoveAt(idx);
                if (idx < targetIdx)
                    targetIdx--;
            }
            for (int i = 0; i < selectedItems.Count; i++)
            {
                this.Items.Insert(targetIdx + i, selectedItems[i]);
            }
        }
        #endregion
    }

}
