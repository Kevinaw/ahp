using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Threading;

namespace MyListControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        // content in the list
        List<string> listContent;
        // indicate status of the list
        bool isEditing;
        int edtIdx;
        public int selectedIdx;
        public int itemsCountLimit = Int32.MaxValue;

        public UserControl1()
        {
            InitializeComponent();
            listContent = new List<string>();
            isEditing = false;

            selectedIdx = -1;
        }

        // add new item
        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing == true)
                return;

            if(contentLbx.Items.Count >= itemsCountLimit)
            {
                MessageBox.Show("You can not add more items as limit is reached!", "Upper limit reached");
                return;
            }

            TextBox newTxtBx = new TextBox();
            newTxtBx.Width = contentLbx.ActualWidth - 10;
            newTxtBx.Name = "addItemBox";
            newTxtBx.LostFocus += addItemBox_LostFocus;
           
            contentLbx.Items.Add(newTxtBx);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
            (Action)(() => { Keyboard.Focus(newTxtBx); }));

            isEditing = true;
            edtIdx = contentLbx.Items.Count - 1;
        }

        // add new item lost focus
        private void addItemBox_LostFocus(object sender, RoutedEventArgs e)
        {
            bool fireAddedEvent = false;
            bool fireUpdatedEvent = false;
            if (isEditing == true)
            {
                TextBox txb = sender as TextBox;
                if (txb == null)
                    return;

                if (txb.Text.Trim() == string.Empty)
                {
                    // delete when adding
                    if(listContent.Count == edtIdx)
                    {
                        // if the textbox is empty, remove it
                        contentLbx.Items.RemoveAt(edtIdx);
                        isEditing = false;
                    }
                    return;
                }                    

                //listAlt.Remove(txt.Text);
                if (listContent.Count > edtIdx) // edit
                {
                    if (listContent[edtIdx] != txb.Text && listContent.Exists(x => x == txb.Text))
                    {
                        MessageBox.Show("duplicate item name!");
                        return;
                    }

                    listContent[edtIdx] = txb.Text;


                    fireUpdatedEvent = true;
                }                    
                else // new
                {
                    if (listContent.Exists(x => x == txb.Text))
                    {
                        MessageBox.Show("duplicate item name!");
                        return;
                    }

                    listContent.Add(txb.Text);

                    fireAddedEvent = true;
                }                    

                TextBlock txblk = new TextBlock();
                txblk.Text = txb.Text;

                Grid newGrid = new Grid();
                ColumnDefinition col1 = new ColumnDefinition();
                ColumnDefinition col2 = new ColumnDefinition();
                col1.Width = new GridLength(contentLbx.ActualWidth * 5 / 8);
                col1.Width = new GridLength(contentLbx.ActualWidth * 3 / 8);
                newGrid.ColumnDefinitions.Add(col1);
                newGrid.ColumnDefinitions.Add(col2);

                TextBlock newText = new TextBlock();
                newText.Text = txb.Text;
                newText.Name = "textblock" + edtIdx.ToString();
                //newText.AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(ListItem_DoubleClick));
                newText.MouseDown += new MouseButtonEventHandler(ListItem_DoubleClick);

                Button newBtn = new Button();
                newBtn.Name = "deleteBtn" + edtIdx.ToString();
                newBtn.Width = 22;
                newBtn.Height = 22;
                newBtn.Cursor = Cursors.Hand;
                newBtn.ToolTip = "Delete";
                newBtn.Click += new RoutedEventHandler(deleteBtn_OnClick);

                ImageBrush newB = new ImageBrush();
                newB.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/delete1.png"));
                newBtn.Background = newB;

                newGrid.Children.Add(newText);
                newGrid.Children.Add(newBtn);

                Grid.SetColumn(newBtn, 1);
                contentLbx.Items.RemoveAt(edtIdx);
                contentLbx.Items.Insert(edtIdx, newGrid);

                isEditing = false;

                if (fireUpdatedEvent) // edit
                {
                    ListItemChangedEventArgs e1 = new ListItemChangedEventArgs();
                    e1.Idx = edtIdx;
                    e1.NewName = txb.Text;
                    onListItemUpdatedEvent(e1);
                }
                else if(fireAddedEvent == true) // new
                {
                    ListItemChangedEventArgs e1 = new ListItemChangedEventArgs();
                    e1.Idx = edtIdx;
                    e1.NewName = txb.Text;
                    onListItemAddedEvent(e1);
                }
            }
        }

        public void Update(int idx, string name)
        {
            Grid grd = contentLbx.Items.GetItemAt(idx) as Grid;
            TextBlock txb = grd.Children[0] as TextBlock;
            if (txb == null)
                txb = grd.Children[1] as TextBlock;
            txb.Text = name;

            listContent[idx] = name;
        }

        private void deleteBtn_OnClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                int i;
                int idx;
                Grid grd = null;
                //ListBoxItem lbi = null;

                idx = Convert.ToInt16(btn.Name.Substring(9));

                grd = contentLbx.Items.GetItemAt(idx) as Grid;


                TextBlock txt = grd.Children[0] as TextBlock;
                if (txt == null)
                    txt = grd.Children[1] as TextBlock;
                listContent.Remove(txt.Text);
                ListItemChangedEventArgs e1 = new ListItemChangedEventArgs();
                e1.Idx = idx;
                e1.NewName = txt.Text;
                onListItemDeletedEvent(e1);
                //drawAhpHierarchy();
                // delete this item according the index
                contentLbx.Items.RemoveAt(idx);

                if (idx != contentLbx.Items.Count)
                {
                    for (i = idx; i < contentLbx.Items.Count; i++)
                    {
                        Grid newGrid = new Grid();
                        ColumnDefinition col1 = new ColumnDefinition();
                        ColumnDefinition col2 = new ColumnDefinition();
                        col1.Width = new GridLength(contentLbx.ActualWidth * 5 / 8);
                        col1.Width = new GridLength(contentLbx.ActualWidth * 3 / 8);
                        newGrid.ColumnDefinitions.Add(col1);
                        newGrid.ColumnDefinitions.Add(col2);

                        TextBlock newText = new TextBlock();
                        newText.Text = listContent[i];
                        newText.Name = "textblock" + i.ToString();
                        //newText.AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(ListItem_DoubleClick));
                        newText.MouseDown += new MouseButtonEventHandler(ListItem_DoubleClick);

                        Button newBtn = new Button();
                        newBtn.Name = "deleteBtn" + i.ToString();
                        newBtn.Width = 22;
                        newBtn.Height = 22;
                        newBtn.Cursor = Cursors.Hand;
                        newBtn.ToolTip = "Delete";
                        newBtn.Click += new RoutedEventHandler(deleteBtn_OnClick);

                        ImageBrush newB = new ImageBrush();
                        newB.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/delete1.png"));
                        newBtn.Background = newB;

                        newGrid.Children.Add(newText);
                        newGrid.Children.Add(newBtn);

                        Grid.SetColumn(newBtn, 1);
                        contentLbx.Items.RemoveAt(i);
                        contentLbx.Items.Insert(i, newGrid);
                    }
                }
            }
        }

        private void ListItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            // double click, trigger this editing event
            if (e.ClickCount != 2)
                return;

            TextBlock txb = sender as TextBlock;
            if (txb == null)
                return;

            TextBox txb1 = new TextBox();
            txb1.Text = txb.Text;
            txb1.LostFocus += addItemBox_LostFocus;

            edtIdx = Convert.ToInt16(txb.Name.Substring(9));


            //listAlt.Remove(txb.Text);
            //drawAhpHierarchy();

            Grid grd = txb.Parent as Grid;
            grd.Children.Remove(txb);
            grd.Children.Add(txb1);

            isEditing = true;
            //ListBoxItem lbi = grd.Parent as ListBoxItem;

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                (Action)(() => { Keyboard.Focus(txb1); }));
        }

        public event EventHandler<ListItemChangedEventArgs> ListItemAddedEvent;
        protected virtual void onListItemAddedEvent(ListItemChangedEventArgs e)
        {
            if (ListItemAddedEvent != null)
                ListItemAddedEvent(this, e);
        }

        public event EventHandler<ListItemChangedEventArgs> ListItemDeletedEvent;
        protected virtual void onListItemDeletedEvent(ListItemChangedEventArgs e)
        {
            if (ListItemDeletedEvent != null)
                ListItemDeletedEvent(this, e);
        }

        public event EventHandler<ListItemChangedEventArgs> ListItemUpdatedEvent;
        protected virtual void onListItemUpdatedEvent(ListItemChangedEventArgs e)
        {
            if (ListItemUpdatedEvent != null)
                ListItemUpdatedEvent(this, e);
        }

        public event EventHandler SelectionChangedEvent;
        protected virtual void onSelectionChangedEvent(EventArgs e)
        {
            if (SelectionChangedEvent != null)
                SelectionChangedEvent(this, e);
        }

        private void contentLbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedIdx = contentLbx.SelectedIndex;
            onSelectionChangedEvent(e);

        }

        public void Clear()
        {
            contentLbx.Items.Clear();
            listContent.Clear();
            isEditing = false;
            edtIdx = 0;
            selectedIdx = -1;
        }

        public void AddItem(string name)
        {

                    listContent.Add(name);

                Grid newGrid = new Grid();
                ColumnDefinition col1 = new ColumnDefinition();
                ColumnDefinition col2 = new ColumnDefinition();
                col1.Width = new GridLength(200 * 5 / 8);
                col1.Width = new GridLength(200 * 3 / 8);
                newGrid.ColumnDefinitions.Add(col1);
                newGrid.ColumnDefinitions.Add(col2);

                TextBlock newText = new TextBlock();
                newText.Text = name;
                newText.Name = "textblock" + edtIdx.ToString();
                newText.MouseDown += new MouseButtonEventHandler(ListItem_DoubleClick);

                Button newBtn = new Button();
                newBtn.Name = "deleteBtn" + edtIdx.ToString();
                newBtn.Width = 22;
                newBtn.Height = 22;
                newBtn.Cursor = Cursors.Hand;
                newBtn.ToolTip = "delete alternative";
                newBtn.Click += new RoutedEventHandler(deleteBtn_OnClick);

                ImageBrush newB = new ImageBrush();
                newB.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/delete1.png"));
                newBtn.Background = newB;

                newGrid.Children.Add(newText);
                newGrid.Children.Add(newBtn);

                Grid.SetColumn(newBtn, 1);
                contentLbx.Items.Add(newGrid);

        }

    }

    public class ListItemChangedEventArgs : EventArgs
    {
        public int Idx { get; set; }
        public string NewName { get; set; }
    }
}
