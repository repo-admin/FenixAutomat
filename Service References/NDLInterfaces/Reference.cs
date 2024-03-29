﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FenixAutomat.NDLInterfaces {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://localhost:8082/upcsw/NDL_Interfaces", ConfigurationName="NDLInterfaces.PortTypeNDL_Interfaces")]
    public interface PortTypeNDL_Interfaces {
        
        // CODEGEN: Generating message contract since element name login from namespace http://localhost:8082/upcsw/NDL_Interfaces is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="", ReplyAction="*")]
        FenixAutomat.NDLInterfaces.UPWSI0Response UPWSI0(FenixAutomat.NDLInterfaces.UPWSI0Request request);
        
        [System.ServiceModel.OperationContractAttribute(Action="", ReplyAction="*")]
        System.Threading.Tasks.Task<FenixAutomat.NDLInterfaces.UPWSI0Response> UPWSI0Async(FenixAutomat.NDLInterfaces.UPWSI0Request request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class UPWSI0Request {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="UPWSI0", Namespace="http://localhost:8082/upcsw/NDL_Interfaces", Order=0)]
        public FenixAutomat.NDLInterfaces.UPWSI0RequestBody Body;
        
        public UPWSI0Request() {
        }
        
        public UPWSI0Request(FenixAutomat.NDLInterfaces.UPWSI0RequestBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://localhost:8082/upcsw/NDL_Interfaces")]
    public partial class UPWSI0RequestBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string login;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=1)]
        public string password;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=2)]
        public string partnerCode;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=3)]
        public string messageType;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=4)]
        public string characterEncoding;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=5)]
        public byte[] image;
        
        public UPWSI0RequestBody() {
        }
        
        public UPWSI0RequestBody(string login, string password, string partnerCode, string messageType, string characterEncoding, byte[] image) {
            this.login = login;
            this.password = password;
            this.partnerCode = partnerCode;
            this.messageType = messageType;
            this.characterEncoding = characterEncoding;
            this.image = image;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class UPWSI0Response {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="UPWSI0Response", Namespace="http://localhost:8082/upcsw/NDL_Interfaces", Order=0)]
        public FenixAutomat.NDLInterfaces.UPWSI0ResponseBody Body;
        
        public UPWSI0Response() {
        }
        
        public UPWSI0Response(FenixAutomat.NDLInterfaces.UPWSI0ResponseBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://localhost:8082/upcsw/NDL_Interfaces")]
    public partial class UPWSI0ResponseBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public byte[] UPWSI0Return;
        
        public UPWSI0ResponseBody() {
        }
        
        public UPWSI0ResponseBody(byte[] UPWSI0Return) {
            this.UPWSI0Return = UPWSI0Return;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface PortTypeNDL_InterfacesChannel : FenixAutomat.NDLInterfaces.PortTypeNDL_Interfaces, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class PortTypeNDL_InterfacesClient : System.ServiceModel.ClientBase<FenixAutomat.NDLInterfaces.PortTypeNDL_Interfaces>, FenixAutomat.NDLInterfaces.PortTypeNDL_Interfaces {
        
        public PortTypeNDL_InterfacesClient() {
        }
        
        public PortTypeNDL_InterfacesClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public PortTypeNDL_InterfacesClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PortTypeNDL_InterfacesClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PortTypeNDL_InterfacesClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        FenixAutomat.NDLInterfaces.UPWSI0Response FenixAutomat.NDLInterfaces.PortTypeNDL_Interfaces.UPWSI0(FenixAutomat.NDLInterfaces.UPWSI0Request request) {
            return base.Channel.UPWSI0(request);
        }
        
        public byte[] UPWSI0(string login, string password, string partnerCode, string messageType, string characterEncoding, byte[] image) {
            FenixAutomat.NDLInterfaces.UPWSI0Request inValue = new FenixAutomat.NDLInterfaces.UPWSI0Request();
            inValue.Body = new FenixAutomat.NDLInterfaces.UPWSI0RequestBody();
            inValue.Body.login = login;
            inValue.Body.password = password;
            inValue.Body.partnerCode = partnerCode;
            inValue.Body.messageType = messageType;
            inValue.Body.characterEncoding = characterEncoding;
            inValue.Body.image = image;
            FenixAutomat.NDLInterfaces.UPWSI0Response retVal = ((FenixAutomat.NDLInterfaces.PortTypeNDL_Interfaces)(this)).UPWSI0(inValue);
            return retVal.Body.UPWSI0Return;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<FenixAutomat.NDLInterfaces.UPWSI0Response> FenixAutomat.NDLInterfaces.PortTypeNDL_Interfaces.UPWSI0Async(FenixAutomat.NDLInterfaces.UPWSI0Request request) {
            return base.Channel.UPWSI0Async(request);
        }
        
        public System.Threading.Tasks.Task<FenixAutomat.NDLInterfaces.UPWSI0Response> UPWSI0Async(string login, string password, string partnerCode, string messageType, string characterEncoding, byte[] image) {
            FenixAutomat.NDLInterfaces.UPWSI0Request inValue = new FenixAutomat.NDLInterfaces.UPWSI0Request();
            inValue.Body = new FenixAutomat.NDLInterfaces.UPWSI0RequestBody();
            inValue.Body.login = login;
            inValue.Body.password = password;
            inValue.Body.partnerCode = partnerCode;
            inValue.Body.messageType = messageType;
            inValue.Body.characterEncoding = characterEncoding;
            inValue.Body.image = image;
            return ((FenixAutomat.NDLInterfaces.PortTypeNDL_Interfaces)(this)).UPWSI0Async(inValue);
        }
    }
}
