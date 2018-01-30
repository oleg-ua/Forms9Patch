﻿using Xamarin.Forms;
using System;
using FormsGestures;

namespace Forms9Patch
{
    /// <summary>
    /// DO NOT USE: Used by Forms9Patch.ListView as a foundation for cells.
    /// </summary>
    class BaseCellView : Xamarin.Forms.Grid  // why grid?  because you can put more than one view in the same place at the same time
    {

        #region Properties
        /// <summary>
        /// The content property.
        /// </summary>
        public static readonly BindableProperty ContentProperty = BindableProperty.Create("Content", typeof(View), typeof(BaseCellView), default(View));
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        public View Content
        {
            get { return (View)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /*
		#region Separator appearance
		/// <summary>
		/// The separator is visible property.
		/// </summary>
		internal static readonly BindableProperty SeparatorIsVisibleProperty = ItemWrapper.SeparatorIsVisibleProperty;
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:Forms9Patch.BaseCellView"/> separator is visible.
		/// </summary>
		/// <value><c>true</c> if separator is visible; otherwise, <c>false</c>.</value>
		internal bool SeparatorIsVisible
		{
			get { return (bool)GetValue(SeparatorIsVisibleProperty); }
			set
			{
				SetValue(SeparatorIsVisibleProperty, value);
			}
		}

		/// <summary>
		/// The separator color property.
		/// </summary>
		internal static readonly BindableProperty SeparatorColorProperty = ItemWrapper.SeparatorColorProperty;
		/// <summary>
		/// Gets or sets the color of the separator.
		/// </summary>
		/// <value>The color of the separator.</value>
		internal Color SeparatorColor
		{
			get { return (Color)GetValue(SeparatorColorProperty); }
			set { SetValue(SeparatorColorProperty, value); }
		}

		/// <summary>
		/// The separator height property.
		/// </summary>
		public static readonly BindableProperty SeparatorHeightProperty = ItemWrapper.SeparatorHeightProperty;
		/// <summary>
		/// Gets or sets the height of the separator.
		/// </summary>
		/// <value>The height of the separator.</value>
		public double SeparatorHeight
		{
			get { return (double)GetValue(SeparatorHeightProperty); }
			set { SetValue(SeparatorHeightProperty, value); }
		}

		/// <summary>
		/// The separator left indent property.
		/// </summary>
		public static readonly BindableProperty SeparatorLeftIndentProperty = ItemWrapper.SeparatorLeftIndentProperty;
		/// <summary>
		/// Gets or sets the separator left indent.
		/// </summary>
		/// <value>The separator left indent.</value>
		public double SeparatorLeftIndent
		{
			get { return (double)GetValue(SeparatorLeftIndentProperty); }
			set { SetValue(SeparatorLeftIndentProperty, value); }
		}

		/// <summary>
		/// The separator right indent property.
		/// </summary>
		public static readonly BindableProperty SeparatorRightIndentProperty = ItemWrapper.SeparatorRightIndentProperty;
		/// <summary>
		/// Gets or sets the separator right indent.
		/// </summary>
		/// <value>The separator right indent.</value>
		public double SeparatorRightIndent
		{
			get { return (double)GetValue(SeparatorRightIndentProperty); }
			set { SetValue(SeparatorRightIndentProperty, value); }
		}
		#endregion
		*/

        #endregion


        #region Fields
        static int _instances;
        internal int ID;


        #region Swipe Menu
        readonly Frame _insetFrame = new Frame
        {
            HasShadow = true,
            ShadowInverted = true,
            BackgroundColor = Color.FromRgb(200, 200, 200),
            Padding = 0,
            Margin = 0,
            OutlineWidth = 0
        };
        readonly Frame _swipeFrame1 = new Frame
        {
            Padding = new Thickness(-1)
        };
        readonly Frame _swipeFrame2 = new Frame
        {
            Padding = new Thickness(-1)
        };
        readonly Frame _swipeFrame3 = new Frame
        {
            Padding = new Thickness(-1)
        };
        readonly Frame _touchBlocker = new Frame
        {
            BackgroundColor = Color.FromRgba(0, 0, 0, 1)
        };

        readonly Button _swipeButton1 = new Button { WidthRequest = 50, OutlineWidth = 0, OutlineRadius = 0, Orientation = StackOrientation.Vertical };
        readonly Button _swipeButton2 = new Button { WidthRequest = 44, OutlineWidth = 0, OutlineRadius = 0, Orientation = StackOrientation.Vertical };
        readonly Button _swipeButton3 = new Button { WidthRequest = 44, OutlineWidth = 0, OutlineRadius = 0, Orientation = StackOrientation.Vertical };

        #endregion

        #endregion


        #region Constructor
        /// <summary>
        /// DO NOT USE: Initializes a new instance of the <see cref="T:Forms9Patch.BaseCellView"/> class.
        /// </summary>
        public BaseCellView()
        {
            ID = _instances++;
            Padding = 0; // new Thickness(0,1,0,1);
            ColumnSpacing = 0;
            RowSpacing = 0;
            Margin = 0;


            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            };
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            };

            var thisListener = FormsGestures.Listener.For(this);
            thisListener.Tapped += OnTapped;
            thisListener.LongPressed += OnLongPressed;
            thisListener.LongPressing += OnLongPressing;
            thisListener.Panned += OnPanned;
            thisListener.Panning += OnPanning;

            _swipeFrame1.Content = _swipeButton1;
            _swipeFrame2.Content = _swipeButton2;
            _swipeFrame3.Content = _swipeButton3;

            _swipeButton1.Tapped += OnSwipeButtonTapped;
            _swipeButton2.Tapped += OnSwipeButtonTapped;
            _swipeButton3.Tapped += OnSwipeButtonTapped;


        }
        #endregion


        #region Swipe Menu
        enum Side
        {
            Start = -1,
            End = 1
        }
        bool _settingup;
        int _endButtons;
        int _startButtons;
        double _translateOnUp;

        double ContentX
        {
            get
            {
                return Content.TranslationX;
            }
            set
            {
                Content.TranslationX = value;
            }
        }

        void TranslateChildrenTo(double x, double y, uint milliseconds, Easing easing)
        {
            Content.TranslateTo(x, y, milliseconds, easing);
        }

        void OnPanned(object sender, PanEventArgs e)
        {
            if (_panVt)
            {
                _panVt = false;
                return;
            }
            _panHz = false;
            var iCellSwipeMenus = Content as ICellSwipeMenus;
            if (iCellSwipeMenus != null)
            {
                double distance = e.TotalDistance.X + _translateOnUp;
                if (_endButtons + _startButtons > 0)
                {
                    var side = _startButtons > 0 ? Side.Start : Side.End;
                    //System.Diagnostics.Debug.WriteLine("ChildrenX=[" + ChildrenX + "]");
                    if ((_endButtons > 0 && side == Side.End && (e.TotalDistance.X > 20 || ContentX > -60)) ||
                        (_startButtons > 0 && side == Side.Start && (e.TotalDistance.X < -20 || ContentX < 60)))
                    {
                        PutAwaySwipeButtons(true);
                        return;
                    }
                    if ((_endButtons > 0 && side == Side.End && /*_swipeFrame1.TranslationX < Width - 210 */ distance <= -210 && ((ICellSwipeMenus)Content)?.EndSwipeMenu != null && ((ICellSwipeMenus)Content).EndSwipeMenu.Count > 0 && ((ICellSwipeMenus)Content).EndSwipeMenu[0].SwipeActivated) ||
                        (_startButtons > 0 && side == Side.Start && /*_swipeFrame1.TranslationX > 210 - Width */ distance >= 210 && ((ICellSwipeMenus)Content)?.StartSwipeMenu != null && ((ICellSwipeMenus)Content).StartSwipeMenu.Count > 0 && ((ICellSwipeMenus)Content).StartSwipeMenu[0].SwipeActivated))
                    {
                        // execute full swipe
                        _swipeFrame1.TranslateTo(0, 0, 250, Easing.Linear);
                        OnSwipeButtonTapped(_swipeButton1, EventArgs.Empty);
                        Device.StartTimer(TimeSpan.FromMilliseconds(400), () =>
                        {
                            PutAwaySwipeButtons(false);
                            return false;
                        });
                    }
                    else
                    {
                        // display 3 buttons
                        TranslateChildrenTo(-(int)side * (60 * (_endButtons + _startButtons)), 0, 300, Easing.Linear);
                        _swipeFrame1.TranslateTo((int)side * (Width - 60), 0, 300, Easing.Linear);
                        if (_endButtons + _startButtons > 1)
                            _swipeFrame2.TranslateTo((int)side * (Width - 120), 0, 300, Easing.Linear);
                        if (_endButtons + _startButtons > 2)
                            _swipeFrame3.TranslateTo((int)side * (Width - 180), 0, 300, Easing.Linear);
                        _insetFrame.TranslateTo((int)side * (Width - (60 * (_endButtons + _startButtons))), 0, 300, Easing.Linear);
                        _translateOnUp = (int)side * -180;
                        return;
                    }
                }

            }
        }

        bool _panHz, _panVt;
        void OnPanning(object sender, PanEventArgs e)
        {
            if (_panVt)
                return;
            if (!_panVt && !_panHz)
            {
                if (Math.Abs(e.TotalDistance.Y) > 10)
                {
                    _panVt = true;
                    return;
                }
                if (Math.Abs(e.TotalDistance.X) > 10)
                    _panHz = true;
                else
                    return;
            }

            double distance = e.TotalDistance.X + _translateOnUp;
            //System.Diagnostics.Debug.WriteLine("eb=["+_endButtons+"] sb=["+startButtons+"] Distance=["+distance+"] translateOnUp=["+translateOnUp+"]");
            if (_settingup)
                return;
            if (_endButtons + _startButtons > 0)
            {
                var side = _startButtons > 0 ? Side.Start : Side.End;
                if ((side == Side.End && distance <= -60 * _endButtons) || (side == Side.Start && distance >= 60 * _startButtons))
                {
                    // we're beyond the limit of presentation of the buttons
                    ContentX = (int)side * -180;
                    if (side == Side.End && distance <= -210 && e.DeltaDistance.X <= 0 && ((ICellSwipeMenus)Content)?.EndSwipeMenu != null && ((ICellSwipeMenus)Content).EndSwipeMenu.Count > 0 && ((ICellSwipeMenus)Content).EndSwipeMenu[0].SwipeActivated)
                        _swipeFrame1.TranslateTo(0, 0, 200, Easing.Linear);
                    else if (side == Side.Start && distance >= 210 && e.DeltaDistance.X >= 0 && ((ICellSwipeMenus)Content)?.StartSwipeMenu != null && ((ICellSwipeMenus)Content).StartSwipeMenu.Count > 0 && ((ICellSwipeMenus)Content).StartSwipeMenu[0].SwipeActivated)
                        _swipeFrame1.TranslateTo(0, 0, 200, Easing.Linear);
                    else
                        _swipeFrame1.TranslateTo((int)side * (Width - 60), 0, 200, Easing.Linear);
                    if (_endButtons + _startButtons > 1)
                        _swipeFrame2.TranslationX = (int)side * (Width - (int)side * 120);
                    if (_endButtons + _startButtons > 2)
                        _swipeFrame3.TranslationX = (int)side * (Width - (int)side * 180);
                    _insetFrame.TranslationX = (int)side * (Width + (int)side * distance);
                    return;
                }
                if ((side == Side.End && distance > 1) || (side == Side.Start && distance < 1))
                {
                    // we keep the endButtons going so as to not allow for the startButtons to appear
                    ContentX = 0;
                    return;
                }
                ContentX = distance;
                _swipeFrame1.TranslationX = (int)side * (Width + (int)side * distance / (_endButtons + _startButtons));
                _swipeFrame2.TranslationX = (int)side * (Width + (int)side * 2 * distance / (_endButtons + _startButtons));
                _swipeFrame3.TranslationX = (int)side * (Width + (int)side * distance);
                _insetFrame.TranslationX = (int)side * (Width + (int)side * distance);
            }
            else if (Math.Abs(distance) > 0.1)
            {
                // setup end SwipeMenu
                var side = distance < 0 ? Side.End : Side.Start;
                var iCellSwipeMenus = Content as ICellSwipeMenus;
                if (iCellSwipeMenus != null)
                {
                    var swipeMenu = side == Side.End ? iCellSwipeMenus.EndSwipeMenu : iCellSwipeMenus.StartSwipeMenu;
                    if (swipeMenu != null && swipeMenu.Count > 0)
                    {
                        _settingup = true;

                        Children.Add(_touchBlocker, 0, 0);
                        _touchBlocker.IsVisible = true;

                        Children.Add(_insetFrame, 0, 0);
                        _insetFrame.TranslationX = (int)side * Width;

                        // setup buttons
                        if (side == Side.End)
                        {
                            _endButtons = 1;
                            _swipeButton1.HorizontalOptions = LayoutOptions.Start;
                        }
                        else
                        {
                            _startButtons = 1;
                            _swipeButton1.HorizontalOptions = LayoutOptions.End;
                        }
                        _translateOnUp = 0;
                        _swipeFrame1.BackgroundColor = swipeMenu[0].BackgroundColor;
                        _swipeButton1.HtmlText = swipeMenu[0].Text;
                        _swipeButton1.IconText = swipeMenu[0].IconText;
                        _swipeButton1.TextColor = swipeMenu[0].TextColor;

                        _swipeFrame2.IsVisible = false;
                        _swipeFrame3.IsVisible = false;

                        if (swipeMenu.Count > 1)
                        {
                            if (side == Side.End)
                            {
                                _endButtons = 2;
                                _swipeButton2.HorizontalOptions = LayoutOptions.Start;
                            }
                            else
                            {
                                _startButtons = 2;
                                _swipeButton2.HorizontalOptions = LayoutOptions.End;
                            }
                            _swipeFrame2.BackgroundColor = swipeMenu[1].BackgroundColor;
                            _swipeButton2.HtmlText = swipeMenu[1].Text;
                            _swipeButton2.IconText = swipeMenu[1].IconText;
                            _swipeButton2.TextColor = swipeMenu[1].TextColor;
                            if (swipeMenu.Count > 2)
                            {
                                if (side == Side.End)
                                {
                                    _endButtons = 3;
                                    _swipeButton3.HorizontalOptions = LayoutOptions.Start;
                                }
                                else
                                {
                                    _startButtons = 3;
                                    _swipeButton3.HorizontalOptions = LayoutOptions.End;
                                }
                                if (swipeMenu.Count > 3)
                                {
                                    _swipeFrame3.BackgroundColor = Color.Gray;
                                    _swipeButton3.HtmlText = "More";
                                    _swipeButton3.IconText = "•••";
                                    _swipeButton3.TextColor = Color.White;
                                }
                                else
                                {
                                    _swipeFrame3.BackgroundColor = swipeMenu[2].BackgroundColor;
                                    _swipeButton3.HtmlText = swipeMenu[2].Text;
                                    _swipeButton3.IconText = swipeMenu[2].IconText;
                                    _swipeButton3.TextColor = swipeMenu[2].TextColor;
                                }
                                Children.Add(_swipeFrame3, 0, 0);
                                RaiseChild(_swipeFrame3);
                                _swipeFrame3.TranslationX = (int)side * Width;
                                _swipeFrame3.IsVisible = true;
                            }
                            Children.Add(_swipeFrame2, 0, 0);
                            RaiseChild(_swipeFrame2);
                            _swipeFrame2.TranslationX = (int)side * (Width - distance / 3.0);
                            _swipeFrame2.IsVisible = true;
                        }
                        Children.Add(_swipeFrame1, 0, 0);
                        RaiseChild(_swipeFrame1);
                        _swipeFrame1.TranslationX = (int)side * (Width - 2 * distance / 3.0);
                        _swipeFrame1.IsVisible = true;
                        _settingup = false;
                    }

                }
            }
        }

        void PutAwaySwipeButtons(bool animated)
        {
            var parkingX = _endButtons > 0 ? Width : -Width;
            if (animated)
            {
                TranslateChildrenTo(0, 0, 300, Easing.Linear);
                _swipeFrame1.TranslateTo(parkingX, 0, 400, Easing.Linear);
                if (_endButtons + _startButtons > 1)
                    _swipeFrame2.TranslateTo(parkingX, 0, 400, Easing.Linear);
                if (_endButtons + _startButtons > 2)
                    _swipeFrame3.TranslateTo(parkingX, 0, 400, Easing.Linear);
                _insetFrame.TranslateTo(parkingX, 0, 400, Easing.Linear);
                Device.StartTimer(TimeSpan.FromMilliseconds(400), () =>
                {
                    _touchBlocker.IsVisible = false;
                    _swipeFrame1.HorizontalOptions = LayoutOptions.Fill;
                    _swipeFrame2.HorizontalOptions = LayoutOptions.Fill;
                    _swipeFrame3.HorizontalOptions = LayoutOptions.Fill;
                    _swipeFrame1.IsVisible = false;
                    _swipeFrame2.IsVisible = false;
                    _swipeFrame3.IsVisible = false;
                    return false;
                });
            }
            else
            {
                ContentX = 0;
                _swipeFrame1.TranslationX = parkingX;
                if (_endButtons + _startButtons > 1)
                    _swipeFrame2.TranslationX = parkingX;
                if (_endButtons + _startButtons > 2)
                    _swipeFrame3.TranslationX = parkingX;
                _insetFrame.TranslationX = parkingX;
                _touchBlocker.IsVisible = false;
            }
            _translateOnUp = 0;
            _endButtons = 0;
            _startButtons = 0;
        }

        void OnSwipeButtonTapped(object sender, EventArgs e)
        {
            int index = 0;
            if (sender == _swipeButton2)
                index = 1;
            else if (sender == _swipeButton3)
                index = 2;
            var swipeMenu = _endButtons > 0 ? ((ICellSwipeMenus)Content)?.EndSwipeMenu : ((ICellSwipeMenus)Content)?.StartSwipeMenu;
            if (index == 2 && _endButtons + _startButtons > 2)
            {
                // show remaining menu items in a modal list

                var segmentedController = new SegmentedControl
                {
                    Orientation = StackOrientation.Vertical,
                    BackgroundColor = Settings.ListViewCellSwipePopupMenuButtonColor,
                    FontSize = Settings.ListViewCellSwipePopupMenuFontSize,
                    TextColor = Settings.ListViewCellSwipePopupMenuTextColor,
                    OutlineColor = Settings.ListViewCellSwipePopupMenuButtonOutlineColor,
                    OutlineWidth = Settings.ListViewCellSwipePopupMenuButtonOutlineWidth,
                    SeparatorWidth = Settings.ListViewCellSwipePopupMenuButtonSeparatorWidth,
                    OutlineRadius = Settings.ListViewCellSwipePopupMenuButtonOutlineRadius,
                    Padding = 5,
                    WidthRequest = Settings.ListViewCellSwipePopupMenuWidthRequest
                };
                var cancelButton = new Button
                {
                    Text = "Cancel",
                    FontAttributes = FontAttributes.Bold,
                    BackgroundColor = Settings.ListViewCellSwipePopupMenuButtonColor,
                    FontSize = Settings.ListViewCellSwipePopupMenuFontSize,
                    TextColor = Settings.ListViewCellSwipePopupMenuTextColor,
                    OutlineColor = Settings.ListViewCellSwipePopupMenuButtonOutlineColor,
                    OutlineWidth = Settings.ListViewCellSwipePopupMenuButtonOutlineWidth,
                    SeparatorWidth = Settings.ListViewCellSwipePopupMenuButtonSeparatorWidth,
                    OutlineRadius = Settings.ListViewCellSwipePopupMenuButtonOutlineRadius,
                    Padding = 5,
                    WidthRequest = Settings.ListViewCellSwipePopupMenuWidthRequest
                };
                var stack = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    WidthRequest = Settings.ListViewCellSwipePopupMenuWidthRequest,
                    Children = { segmentedController, cancelButton }
                };
                var modal = new ModalPopup()
                {
                    BackgroundColor = Color.Transparent,
                    OutlineWidth = 0,
                    WidthRequest = Settings.ListViewCellSwipePopupMenuWidthRequest,
                    Content = stack
                };
                cancelButton.Tapped += (s, arg) => modal.Cancel();
                for (int i = 2; i < swipeMenu.Count; i++)
                {
                    var menuItem = swipeMenu[i];
                    var segment = new Segment
                    {
                        Text = menuItem.Text,
                        IconText = menuItem.IconText,
                        //ImageSource = menuItem.ImageSource
                        IconImage = new Image { Source = menuItem.ImageSource }
                    };
                    segment.Tapped += (s, arg) =>
                    {
                        modal.Cancel();
                        var args = new SwipeMenuItemTappedArgs((ICellSwipeMenus)Content, (ItemWrapper)BindingContext, menuItem);
                        ((ICellSwipeMenus)Content)?.OnSwipeMenuItemButtonTapped(this, args);
                        ((ItemWrapper)BindingContext)?.OnSwipeMenuItemTapped(this, args);
                        //System.Diagnostics.Debug.WriteLine("SwipeMenu[" + menuItem.Key + "]");
                    };
                    segmentedController.Segments.Add(segment);
                }
                modal.IsVisible = true;
                //System.Diagnostics.Debug.WriteLine("SwipeMenu[More]");
            }
            else
            {
                PutAwaySwipeButtons(false);
                var args = new SwipeMenuItemTappedArgs((ICellSwipeMenus)Content, (ItemWrapper)BindingContext, swipeMenu[index]);
                ((ICellSwipeMenus)Content)?.OnSwipeMenuItemButtonTapped(this.BindingContext, args);
                ((ItemWrapper)BindingContext)?.OnSwipeMenuItemTapped(this, args);
                //System.Diagnostics.Debug.WriteLine("SwipeMenu[" + swipeMenu[index].Key + "]");
            }
        }

        #endregion


        #region Cell Gestures
        void OnTapped(object sender, TapEventArgs e)
        {
            if (_endButtons + _startButtons == 0)
                ((ItemWrapper)BindingContext)?.OnTapped(this, new ItemWrapperTapEventArgs((ItemWrapper)BindingContext));
        }

        void OnLongPressed(object sender, LongPressEventArgs e)
        {
            if (_endButtons + _startButtons == 0)
                ((ItemWrapper)BindingContext)?.OnLongPressed(this, new ItemWrapperLongPressEventArgs((ItemWrapper)BindingContext));
        }

        void OnLongPressing(object sender, LongPressEventArgs e)
        {
            if (_endButtons + _startButtons == 0)
                ((ItemWrapper)BindingContext)?.OnLongPressing(this, new ItemWrapperLongPressEventArgs((ItemWrapper)BindingContext));
        }
        #endregion


        #region change management

        /// <summary>
        /// Triggered by a change in the binding context
        /// </summary>
        protected override void OnBindingContextChanged()
        {
            //System.Diagnostics.Debug.WriteLine("BaseCellView.OnBindingContextChanged");
            if (BindingContext == null)
                return;
            var item = BindingContext as ItemWrapper;
            if (item != null)
            {
                item.BaseCellView = this;
                item.PropertyChanged += OnItemPropertyChanged;
                UpdateBackground();
                SetHeights();
            }
            //else
            //	System.Diagnostics.Debug.WriteLine("");
            var type = BindingContext?.GetType();
            if (type == typeof(NullItemWrapper) || type == typeof(BlankItemWrapper))
                Content.BindingContext = item;
            else
            {
                Content.BindingContext = item?.Source;
                //System.Diagnostics.Debug.WriteLine("item.Index=[" + item.Index + "] item.Source=[" + item.Source + "] item.SeparatorIsVisible[" + item.SeparatorIsVisible + "]");
            }
            //_freshContent = true;
            //UpdateLayout();
            //System.Diagnostics.Debug.WriteLine("OnBindingContextChanged");
            var selectableContent = Content as IIsSelectedAble;
            if (selectableContent != null)
                selectableContent.IsSelected = ((ItemWrapper)BindingContext).IsSelected;
            base.OnBindingContextChanged();
        }

        protected override void OnPropertyChanging(string propertyName = null)
        {
            //System.Diagnostics.Debug.WriteLine("BaseCellView.OnPropertyChanging("+propertyName+")");
            base.OnPropertyChanging(propertyName);
            if (propertyName == BindingContextProperty.PropertyName)
            {
                var item = BindingContext as ItemWrapper;
                if (item != null)
                    item.PropertyChanged -= OnItemPropertyChanged;
                //_startAccessory.HtmlText = null;
                //_endAccessory.HtmlText = null;
                PutAwaySwipeButtons(false);
            }
            else if (propertyName == ContentProperty.PropertyName && Content != null)
                Children.Remove(Content);
            /*
			else if (propertyName == StartAccessoryProperty.PropertyName && StartAccessory != null)
				StartAccessory.PropertyChanged -= OnStartAccessoryPropertyChanged;
			else if (propertyName == EndAccessoryProperty.PropertyName && EndAccessory != null)
				EndAccessory.PropertyChanged -= OnEndAccessoryPropertyChanged;
				*/
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            //System.Diagnostics.Debug.WriteLine("BaseCellView.OnPropertyChanged(" + propertyName + ")");
            base.OnPropertyChanged(propertyName);
            if (propertyName == ContentProperty.PropertyName && Content != null)
                Children.Add(Content, 0, 0);
        }

        void OnItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("BaseCellView.OnItemPropertyChanging(" + e.PropertyName + ")");
            if (e.PropertyName == ItemWrapper.CellBackgroundColorProperty.PropertyName
                || e.PropertyName == ItemWrapper.SelectedCellBackgroundColorProperty.PropertyName
                || e.PropertyName == ItemWrapper.IndexProperty.PropertyName
               )
                UpdateBackground();

            if (e.PropertyName == ItemWrapper.IsSelectedProperty.PropertyName)
            {
                UpdateBackground();
                var selectableContent = Content as IIsSelectedAble;
                if (selectableContent != null)
                    selectableContent.IsSelected = ((ItemWrapper)BindingContext).IsSelected;
            }

            if (e.PropertyName == ItemWrapper.RowHeightProperty.PropertyName)
                SetHeights();

            //System.Diagnostics.Debug.WriteLine("OnItemPropertyChanged");
            //_freshContent = (_freshContent || e.PropertyName == ContentProperty.PropertyName);
            //UpdateLayout();
        }

        void UpdateBackground()
        {
            var item = BindingContext as ItemWrapper;
            if (item != null)
                BackgroundColor = item.IsSelected ? item.SelectedCellBackgroundColor : item.CellBackgroundColor;
            else
                BackgroundColor = Color.Transparent;
        }

        void SetHeights()
        {
            var content = Content as ICellHeight;
            if (content != null)
            {
                if (content.CellHeight > 0)
                {
                    HeightRequest = content.CellHeight + 1;
                    RowDefinitions[0] = new RowDefinition { Height = new GridLength(content.CellHeight, GridUnitType.Absolute) };
                }
                else
                {
                    var itemWrapper = BindingContext as ItemWrapper;
                    if (itemWrapper != null)
                    {
                        HeightRequest = itemWrapper.RowHeight + 1;
                        RowDefinitions[0] = new RowDefinition { Height = new GridLength(itemWrapper.RowHeight, GridUnitType.Absolute) };
                    }
                    else
                    {
                        HeightRequest = -1;
                        Content.HeightRequest = -1;
                        RowDefinitions[0] = new RowDefinition { Height = GridLength.Auto };
                    }
                }
            }
            //System.Diagnostics.Debug.WriteLine("HeightRequest = [" + HeightRequest + "]");
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            //System.Diagnostics.Debug.WriteLine("BaseCellView.LayoutChildren");
            base.LayoutChildren(x, y, width, height);
        }
        #endregion
    }
}
