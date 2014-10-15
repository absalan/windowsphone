﻿using GoedWare.Windows.Phone.Controls;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Telerik.Windows.Controls;
using Windows.Storage;

namespace MegaApp.Pages
{
    public partial class MainPage : PhoneApplicationPage
    {
        private NavigationParameter _navParam;

        public MainPage()
        {
            this.DataContext = App.CloudDrive;

            InitializeComponent();

            InteractionEffectManager.AllowedTypes.Add(typeof (RadDataBoundListBoxItem));

            // node tap item animation
            var transition = new RadTileTransition();
            this.SetValue(RadTransitionControl.TransitionProperty, transition);
            this.SetValue(RadTileAnimation.ContainerToAnimateProperty, LstCloudDrive);

            BreadCrumbControl.OnBreadCrumbTap += BreadCrumbControlOnOnBreadCrumbTap;
            BreadCrumbControl.OnHomeTap += BreadCrumbControlOnOnHomeTap;
        }

        private void BreadCrumbControlOnOnHomeTap(object sender, EventArgs eventArgs)
        {
            App.CloudDrive.GoToRoot();
        }

        private void BreadCrumbControlOnOnBreadCrumbTap(object sender, BreadCrumbTapEventArgs e)
        {
            App.CloudDrive.GoToFolder(e.Item as NodeViewModel);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.CloudDrive.MoveItemMode)
            {
                this.ApplicationBar = (ApplicationBar)Resources["MoveItemMenu"];
                App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.MoveMenu);
            }
            else
            {
                this.ApplicationBar = (ApplicationBar)Resources["CloudDriveMenu"];
                App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.CloudDriveMenu);
            }

            _navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);
            
            if (e.NavigationMode == NavigationMode.Back)
            {
                if (!App.CloudDrive.NoFolderUpAction)
                    App.CloudDrive.GoFolderUp();
                 
                _navParam = NavigationParameter.Browsing;
            }

            App.CloudDrive.NoFolderUpAction = false;

            switch (_navParam)
            {
                case NavigationParameter.Login:
                {
                    // Remove the login page from the stack. If user presses back button it will then exit the application
                    NavigationService.RemoveBackEntry();
                    
                    App.CloudDrive.FetchNodes();
                    break;
                }
                case NavigationParameter.BreadCrumb:
                {
                    int breadCrumbs = App.CloudDrive.CountBreadCrumbs();
                    for (int x = 0; x <= breadCrumbs; x++)
                        NavigationService.RemoveBackEntry();
                    break;
                }
                case NavigationParameter.ImportLinkLaunch:
                {
                    //App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession), new FastLoginRequestListener());
                    //App.CloudDrive.FetchNodes();

                    //App.CloudDrive.ImportLink(NavigationContext.QueryString["link"]);
                    break;
                }
                case NavigationParameter.Unknown:
                {
                    if (!SettingsService.LoadSetting<bool>(SettingsResources.RememberMe))
                    {
                        NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                        return;
                    }
                    else
                    {
                        App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession), new FastLoginRequestListener(App.CloudDrive));
                    }
                    break;
                }
            }

            base.OnNavigatedTo(e);
        }

        private void OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            if(e.Item == null || e.Item.DataContext == null) return;
            if (e.Item.DataContext as NodeViewModel == null) return;

            this.SetValue(RadTileAnimation.ElementToDelayProperty, e.Item);
            
            App.CloudDrive.OnNodeTap(e.Item.DataContext as NodeViewModel);
        }

        private void OnMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            var focusedListBoxItem = e.FocusedElement as RadDataBoundListBoxItem;
            if (focusedListBoxItem == null || focusedListBoxItem.DataContext == null || !(focusedListBoxItem.DataContext is NodeViewModel))
            {
                // We don't want to open the menu if the focused element is not a list box item.
                // If the list box is empty focusedItem will be null.
                e.Cancel = true;
            }
            else
            {
                App.CloudDrive.FocusedNode = focusedListBoxItem.DataContext as NodeViewModel;
                var visibility = App.CloudDrive.FocusedNode.Type == MNodeType.TYPE_FILE ? Visibility.Visible : Visibility.Collapsed;
                BtnGetPreviewLink.Visibility = visibility;
                BtnDownloadItemCloud.Visibility = visibility;
            }
        }

        private void OnListLoaded(object sender, RoutedEventArgs e)
        {
            if (_navParam != NavigationParameter.Browsing && _navParam != NavigationParameter.BreadCrumb) return;
            
            // Load nodes in the onlistloaded event so that the nodes will display after the back animation and not before
            App.CloudDrive.LoadNodes();
        }
        private void OnRefreshClick(object sender, EventArgs e)
        {
            FileService.ClearFiles(
                NodeService.GetFiles(App.CloudDrive.ChildNodes,
                Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.ThumbnailsDirectory)));
            App.CloudDrive.FetchNodes(App.CloudDrive.CurrentRootNode);
        }
        private void OnAddFolderClick(object sender, EventArgs e)
        {
            App.CloudDrive.AddFolder(App.CloudDrive.CurrentRootNode);
        }

        private void OnOpenLinkClick(object sender, EventArgs e)
        {
            App.CloudDrive.OpenLink();
        }
        private void OnMyAccountClick(object sender, EventArgs e)
        {
            App.CloudDrive.GoToAccountDetails();
        }

        private void OnTransfersClick(object sender, EventArgs e)
        {
            App.CloudDrive.GoToTransfers();
        }

        private void OnCloudUploadClick(object sender, EventArgs e)
        {
            DialogService.ShowUploadOptions(App.CloudDrive);
        }

        private void OnCancelMoveClick(object sender, EventArgs e)
        {
            App.CloudDrive.MoveItemMode = false;
            App.CloudDrive.FocusedNode = null;
            this.ApplicationBar = (ApplicationBar)Resources["CloudDriveMenu"];
            App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.CloudDriveMenu);
        }
        private void OnAcceptMoveClick(object sender, EventArgs e)
        {
            App.CloudDrive.MoveItem(App.CloudDrive.CurrentRootNode);
            App.CloudDrive.MoveItemMode = false;
            this.ApplicationBar = (ApplicationBar)Resources["CloudDriveMenu"];
            App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.CloudDriveMenu);
        }

        private void OnMoveItemTap(object sender, ContextMenuItemSelectedEventArgs e)
        {
            this.ApplicationBar = (ApplicationBar)Resources["MoveItemMenu"];
            App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.MoveMenu);
            App.CloudDrive.MoveItemMode = true;
        }

        private void OnItemStateChanged(object sender, ItemStateChangedEventArgs e)
        {
            App.CloudDrive.UiService.RefreshViewport(LstCloudDrive.ViewportItems);
        }

        private void OnScrollStateChanged(object sender, ScrollStateChangedEventArgs e)
        {
            App.CloudDrive.UiService.RefreshViewport(LstCloudDrive.ViewportItems);
        }

        private void OnGoToTopTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!App.CloudDrive.HasChildNodes()) return;
            
            LstCloudDrive.BringIntoView(App.CloudDrive.ChildNodes.First());
            
            App.CloudDrive.UiService.RefreshViewport(LstCloudDrive.ViewportItems);
        }

        private void OnGoToBottomTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!App.CloudDrive.HasChildNodes()) return;
           
            LstCloudDrive.BringIntoView(App.CloudDrive.ChildNodes.Last());

            App.CloudDrive.UiService.RefreshViewport(LstCloudDrive.ViewportItems);
        }
    }
    
}