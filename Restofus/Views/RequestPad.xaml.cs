﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System;
using System.Collections;
using ReactiveUI;
using System.Reactive;
using Restofus.Components;
using System.Threading.Tasks;
using System.Net.Http;
using Restofus.Utils;
using Restofus.Networking;
using System.Reactive.Linq;
using Restofus.Navigation;
using System.IO;

namespace Restofus.Views
{
    public class RequestPad : UserControl<RequestPad.Context>
    {
        public RequestPad()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public class Context : BaseContext
        {
            IDisposable navigationSubscription;

            Navigator navigator;
            RequestDispatcher httpDispatcher;
            ReactiveRequestSerializer requestSerializer;

            public Context(
                IResolver resolver,
                Navigator navigator,
                RequestDispatcher httpDispatcher,
                ReactiveRequestSerializer requestSerializer,
                QueryEditor.Context queryEditorContext,
                HeadersEditor.Context headersEditorContext) : base (resolver)
            {
                this.navigator = navigator;
                this.httpDispatcher = httpDispatcher;
                this.requestSerializer = requestSerializer;

                QueryEditorContext = queryEditorContext;
                HeadersEditorContext = headersEditorContext;
                
                navigationSubscription = navigator
                    .GetNavigationObservable()
                    .Where(f => f != null)
                    .Select(CreateRequest)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(ObserveRequest);

                Methods = ReactiveMethodCollection.CreateDefault();

                SendCommand = ReactiveCommand.CreateAsyncTask(SendRequest);
            }

            ReactiveRequest CreateRequest(ReactiveFile file)
            {
                return requestSerializer.Deserialize(new FileInfo(file.Path));
            }

            void ObserveRequest(ReactiveRequest request)
            {
                Request = request;
            }

            Task SendRequest(object arg)
            {
                return httpDispatcher.Dispatch(Request?.Clone());
            }

            ReactiveRequest request;
            public ReactiveRequest Request
            {
                get => request;
                set => this.RaiseAndSetIfChanged(ref request, value);
            }

            public ReactiveMethodCollection Methods { get; }

            public ReactiveCommand<Unit> SendCommand { get; }

            public QueryEditor.Context QueryEditorContext { get; }

            public HeadersEditor.Context HeadersEditorContext { get; }
        }
        
    }
    
}
