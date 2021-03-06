﻿using Kooboo.Api;
using Kooboo.Api.ApiResponse;
using Kooboo.Api.Methods;
using Kooboo.Web.Payment;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kooboo.Web.Api.Implementation.Commerce
{
    public class PaymentCallBackApi : IApi, IDynamicApi
    {
        public string ModelName => "Paymentcallback";

        public bool RequireSite => false;

        public bool RequireUser => false;

        public DynamicApi GetMethod(string name)
        {
            int index = name.IndexOf("_");
            if (index == -1)
            {
                return null;
            }

            var paymentname = name.Substring(0, index);
            var methodname = name.Substring(index + 1);

            var paymentmethod = PaymentManager.GetMethod(paymentname);
            if (paymentmethod != null)
            {
                DynamicApi api = new DynamicApi();
                api.Type = this.GetType();
                api.Method = this.GetType().GetMethod("Callback");
                return api;
            }
            return null;
        }

        public IResponse Callback(ApiCall call)
        {
            string name = call.Command.Method;

            int index = name.IndexOf("_");
            if (index == -1)
            {
                return null;
            }

            var paymentname = name.Substring(0, index);
            var methodname = name.Substring(index + 1);

            var paymentmethod = PaymentManager.GetMethod(paymentname);

            if (paymentmethod != null)
            {
                var method = paymentmethod.GetType().GetMethod(methodname);
                if (method != null)
                {
                    object[] para = new object[1];
                    para[0] = call.Context;
                    var result = method.Invoke(paymentmethod, para) as PaymentCallback;

                    if (result != null)
                    {
                        Kooboo.Web.Payment.PaymentManager.CallBack(result, call.Context);

                        if (result.CallbackResponse != null)
                        {
                            return result.CallbackResponse;
                        }

                        else
                        { 
                            var response = new PlainResponse();
                            response.ContentType = "text/html";
                            response.Content = ""; 
                            return response; 
                        }

                    }
                }
            }

            return null;
        }

    }
}
