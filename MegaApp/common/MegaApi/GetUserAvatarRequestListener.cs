﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class GetUserAvatarRequestListener : BaseRequestListener
    {
        private readonly UserDataViewModel _userData;

        public GetUserAvatarRequestListener(UserDataViewModel userData)
        {
            _userData = userData;
        }

        protected override string ProgressMessage
        {
            get { return ProgressMessages.GetUserData; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.GetUserDataFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.GetUserDataFailed_Title; }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string SuccessMessageTitle
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return false; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { throw new NotImplementedException(); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { throw new NotImplementedException(); }
        }

        #region Override Methods

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);
            });

            if(e.getErrorCode() == MErrorType.API_OK)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _userData.HasAvatarImage = true;

                    var img = new BitmapImage();
                    img.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    img.UriSource = new Uri(request.getFile());
                    _userData.AvatarUri = img.UriSource;
                });
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _userData.HasAvatarImage = false;
                    _userData.AvatarUri = null;
                });
            }

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (App.UserData != null)
                    App.UserData.AvatarUri = _userData.AvatarUri;
            });
        }

        #endregion
    }
}
