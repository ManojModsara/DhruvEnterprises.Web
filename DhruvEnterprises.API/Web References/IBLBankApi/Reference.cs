﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace DhruvEnterprises.API.IBLBankApi {
    using System.Diagnostics;
    using System;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.Web.Services.Protocols;
    using System.Web.Services;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="BasicHttpBinding_IIBLeTender", Namespace="http://tempuri.org/")]
    public partial class IBLeTender : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback GetDataOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetTransactionTypeDataOperationCompleted;
        
        private System.Threading.SendOrPostCallback FetchIecDataOperationCompleted;
        
        private System.Threading.SendOrPostCallback UpdateClientResponseOperationCompleted;
        
        private System.Threading.SendOrPostCallback UTREnquiryOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetIecDataOperationCompleted;
        
        private System.Threading.SendOrPostCallback UpdateIecResponseOperationCompleted;
        
        private System.Threading.SendOrPostCallback UTRIecEnquiryOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public IBLeTender() {
            this.Url = global::DhruvEnterprises.API.Properties.Settings.Default.EZYTM_API_IBLBankApi_IBLeTender;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event GetDataCompletedEventHandler GetDataCompleted;
        
        /// <remarks/>
        public event GetTransactionTypeDataCompletedEventHandler GetTransactionTypeDataCompleted;
        
        /// <remarks/>
        public event FetchIecDataCompletedEventHandler FetchIecDataCompleted;
        
        /// <remarks/>
        public event UpdateClientResponseCompletedEventHandler UpdateClientResponseCompleted;
        
        /// <remarks/>
        public event UTREnquiryCompletedEventHandler UTREnquiryCompleted;
        
        /// <remarks/>
        public event GetIecDataCompletedEventHandler GetIecDataCompleted;
        
        /// <remarks/>
        public event UpdateIecResponseCompletedEventHandler UpdateIecResponseCompleted;
        
        /// <remarks/>
        public event UTRIecEnquiryCompletedEventHandler UTRIecEnquiryCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/IIBLeTender/GetData", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string GetData() {
            object[] results = this.Invoke("GetData", new object[0]);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetDataAsync() {
            this.GetDataAsync(null);
        }
        
        /// <remarks/>
        public void GetDataAsync(object userState) {
            if ((this.GetDataOperationCompleted == null)) {
                this.GetDataOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetDataOperationCompleted);
            }
            this.InvokeAsync("GetData", new object[0], this.GetDataOperationCompleted, userState);
        }
        
        private void OnGetDataOperationCompleted(object arg) {
            if ((this.GetDataCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetDataCompleted(this, new GetDataCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/IIBLeTender/GetTransactionTypeData", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string GetTransactionTypeData([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string strTranType) {
            object[] results = this.Invoke("GetTransactionTypeData", new object[] {
                        strTranType});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetTransactionTypeDataAsync(string strTranType) {
            this.GetTransactionTypeDataAsync(strTranType, null);
        }
        
        /// <remarks/>
        public void GetTransactionTypeDataAsync(string strTranType, object userState) {
            if ((this.GetTransactionTypeDataOperationCompleted == null)) {
                this.GetTransactionTypeDataOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetTransactionTypeDataOperationCompleted);
            }
            this.InvokeAsync("GetTransactionTypeData", new object[] {
                        strTranType}, this.GetTransactionTypeDataOperationCompleted, userState);
        }
        
        private void OnGetTransactionTypeDataOperationCompleted(object arg) {
            if ((this.GetTransactionTypeDataCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetTransactionTypeDataCompleted(this, new GetTransactionTypeDataCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/IIBLeTender/FetchIecData", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public FetchIecDataResponse FetchIecData([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] FetchIecDataRequest RequestData) {
            object[] results = this.Invoke("FetchIecData", new object[] {
                        RequestData});
            return ((FetchIecDataResponse)(results[0]));
        }
        
        /// <remarks/>
        public void FetchIecDataAsync(FetchIecDataRequest RequestData) {
            this.FetchIecDataAsync(RequestData, null);
        }
        
        /// <remarks/>
        public void FetchIecDataAsync(FetchIecDataRequest RequestData, object userState) {
            if ((this.FetchIecDataOperationCompleted == null)) {
                this.FetchIecDataOperationCompleted = new System.Threading.SendOrPostCallback(this.OnFetchIecDataOperationCompleted);
            }
            this.InvokeAsync("FetchIecData", new object[] {
                        RequestData}, this.FetchIecDataOperationCompleted, userState);
        }
        
        private void OnFetchIecDataOperationCompleted(object arg) {
            if ((this.FetchIecDataCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.FetchIecDataCompleted(this, new FetchIecDataCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/IIBLeTender/UpdateClientResponse", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public UpdateClientResponse UpdateClientResponse([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] UpdateClientResponseRequest RequestData) {
            object[] results = this.Invoke("UpdateClientResponse", new object[] {
                        RequestData});
            return ((UpdateClientResponse)(results[0]));
        }
        
        /// <remarks/>
        public void UpdateClientResponseAsync(UpdateClientResponseRequest RequestData) {
            this.UpdateClientResponseAsync(RequestData, null);
        }
        
        /// <remarks/>
        public void UpdateClientResponseAsync(UpdateClientResponseRequest RequestData, object userState) {
            if ((this.UpdateClientResponseOperationCompleted == null)) {
                this.UpdateClientResponseOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUpdateClientResponseOperationCompleted);
            }
            this.InvokeAsync("UpdateClientResponse", new object[] {
                        RequestData}, this.UpdateClientResponseOperationCompleted, userState);
        }
        
        private void OnUpdateClientResponseOperationCompleted(object arg) {
            if ((this.UpdateClientResponseCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.UpdateClientResponseCompleted(this, new UpdateClientResponseCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/IIBLeTender/UTREnquiry", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public UTREnquiryResponse UTREnquiry([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] UTREnquiryRequest RequestData) {
            object[] results = this.Invoke("UTREnquiry", new object[] {
                        RequestData});
            return ((UTREnquiryResponse)(results[0]));
        }
        
        /// <remarks/>
        public void UTREnquiryAsync(UTREnquiryRequest RequestData) {
            this.UTREnquiryAsync(RequestData, null);
        }
        
        /// <remarks/>
        public void UTREnquiryAsync(UTREnquiryRequest RequestData, object userState) {
            if ((this.UTREnquiryOperationCompleted == null)) {
                this.UTREnquiryOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUTREnquiryOperationCompleted);
            }
            this.InvokeAsync("UTREnquiry", new object[] {
                        RequestData}, this.UTREnquiryOperationCompleted, userState);
        }
        
        private void OnUTREnquiryOperationCompleted(object arg) {
            if ((this.UTREnquiryCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.UTREnquiryCompleted(this, new UTREnquiryCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/IIBLeTender/GetIecData", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string GetIecData([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string CustomerTenderId) {
            object[] results = this.Invoke("GetIecData", new object[] {
                        CustomerTenderId});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetIecDataAsync(string CustomerTenderId) {
            this.GetIecDataAsync(CustomerTenderId, null);
        }
        
        /// <remarks/>
        public void GetIecDataAsync(string CustomerTenderId, object userState) {
            if ((this.GetIecDataOperationCompleted == null)) {
                this.GetIecDataOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetIecDataOperationCompleted);
            }
            this.InvokeAsync("GetIecData", new object[] {
                        CustomerTenderId}, this.GetIecDataOperationCompleted, userState);
        }
        
        private void OnGetIecDataOperationCompleted(object arg) {
            if ((this.GetIecDataCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetIecDataCompleted(this, new GetIecDataCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/IIBLeTender/UpdateIecResponse", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string UpdateIecResponse([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string TenderID, [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string RequestData) {
            object[] results = this.Invoke("UpdateIecResponse", new object[] {
                        TenderID,
                        RequestData});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void UpdateIecResponseAsync(string TenderID, string RequestData) {
            this.UpdateIecResponseAsync(TenderID, RequestData, null);
        }
        
        /// <remarks/>
        public void UpdateIecResponseAsync(string TenderID, string RequestData, object userState) {
            if ((this.UpdateIecResponseOperationCompleted == null)) {
                this.UpdateIecResponseOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUpdateIecResponseOperationCompleted);
            }
            this.InvokeAsync("UpdateIecResponse", new object[] {
                        TenderID,
                        RequestData}, this.UpdateIecResponseOperationCompleted, userState);
        }
        
        private void OnUpdateIecResponseOperationCompleted(object arg) {
            if ((this.UpdateIecResponseCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.UpdateIecResponseCompleted(this, new UpdateIecResponseCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/IIBLeTender/UTRIecEnquiry", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string UTRIecEnquiry([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string UTR, [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string RefNo, [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string CustomerTenderId) {
            object[] results = this.Invoke("UTRIecEnquiry", new object[] {
                        UTR,
                        RefNo,
                        CustomerTenderId});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void UTRIecEnquiryAsync(string UTR, string RefNo, string CustomerTenderId) {
            this.UTRIecEnquiryAsync(UTR, RefNo, CustomerTenderId, null);
        }
        
        /// <remarks/>
        public void UTRIecEnquiryAsync(string UTR, string RefNo, string CustomerTenderId, object userState) {
            if ((this.UTRIecEnquiryOperationCompleted == null)) {
                this.UTRIecEnquiryOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUTRIecEnquiryOperationCompleted);
            }
            this.InvokeAsync("UTRIecEnquiry", new object[] {
                        UTR,
                        RefNo,
                        CustomerTenderId}, this.UTRIecEnquiryOperationCompleted, userState);
        }
        
        private void OnUTRIecEnquiryOperationCompleted(object arg) {
            if ((this.UTRIecEnquiryCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.UTRIecEnquiryCompleted(this, new UTRIecEnquiryCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9032.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.datacontract.org/2004/07/ETender_Pull")]
    public partial class FetchIecDataRequest {
        
        private string customerTenderIdField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string CustomerTenderId {
            get {
                return this.customerTenderIdField;
            }
            set {
                this.customerTenderIdField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9032.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.datacontract.org/2004/07/ETender_Pull")]
    public partial class UTREnquiryResponse {
        
        private string responseCodeField;
        
        private string responseDescriptionField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string ResponseCode {
            get {
                return this.responseCodeField;
            }
            set {
                this.responseCodeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string ResponseDescription {
            get {
                return this.responseDescriptionField;
            }
            set {
                this.responseDescriptionField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9032.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.datacontract.org/2004/07/ETender_Pull")]
    public partial class UTREnquiryRequest {
        
        private string customerTenderIdField;
        
        private string refNoField;
        
        private string uTRField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string CustomerTenderId {
            get {
                return this.customerTenderIdField;
            }
            set {
                this.customerTenderIdField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string RefNo {
            get {
                return this.refNoField;
            }
            set {
                this.refNoField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string UTR {
            get {
                return this.uTRField;
            }
            set {
                this.uTRField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9032.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.datacontract.org/2004/07/ETender_Pull")]
    public partial class UpdateClientResponse {
        
        private string responseCodeField;
        
        private string responseDescriptionField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string ResponseCode {
            get {
                return this.responseCodeField;
            }
            set {
                this.responseCodeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string ResponseDescription {
            get {
                return this.responseDescriptionField;
            }
            set {
                this.responseDescriptionField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9032.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.datacontract.org/2004/07/ETender_Pull")]
    public partial class UpdateClientResponseRequest {
        
        private string customerResponseField;
        
        private string customerTenderIdField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string CustomerResponse {
            get {
                return this.customerResponseField;
            }
            set {
                this.customerResponseField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string CustomerTenderId {
            get {
                return this.customerTenderIdField;
            }
            set {
                this.customerTenderIdField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9032.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.datacontract.org/2004/07/ETender_Pull")]
    public partial class FetchIecDataResponse {
        
        private string responseCodeField;
        
        private string responseDescriptionField;
        
        private string responseIecDataField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string ResponseCode {
            get {
                return this.responseCodeField;
            }
            set {
                this.responseCodeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string ResponseDescription {
            get {
                return this.responseDescriptionField;
            }
            set {
                this.responseDescriptionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string ResponseIecData {
            get {
                return this.responseIecDataField;
            }
            set {
                this.responseIecDataField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void GetDataCompletedEventHandler(object sender, GetDataCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetDataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetDataCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void GetTransactionTypeDataCompletedEventHandler(object sender, GetTransactionTypeDataCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetTransactionTypeDataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetTransactionTypeDataCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void FetchIecDataCompletedEventHandler(object sender, FetchIecDataCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class FetchIecDataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal FetchIecDataCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public FetchIecDataResponse Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((FetchIecDataResponse)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void UpdateClientResponseCompletedEventHandler(object sender, UpdateClientResponseCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class UpdateClientResponseCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal UpdateClientResponseCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public UpdateClientResponse Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((UpdateClientResponse)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void UTREnquiryCompletedEventHandler(object sender, UTREnquiryCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class UTREnquiryCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal UTREnquiryCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public UTREnquiryResponse Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((UTREnquiryResponse)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void GetIecDataCompletedEventHandler(object sender, GetIecDataCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetIecDataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetIecDataCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void UpdateIecResponseCompletedEventHandler(object sender, UpdateIecResponseCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class UpdateIecResponseCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal UpdateIecResponseCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void UTRIecEnquiryCompletedEventHandler(object sender, UTRIecEnquiryCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class UTRIecEnquiryCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal UTRIecEnquiryCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591