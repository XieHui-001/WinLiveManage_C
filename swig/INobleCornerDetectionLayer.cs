//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class INobleCornerDetectionLayer : ILayer {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal INobleCornerDetectionLayer(global::System.IntPtr cPtr, bool cMemoryOwn) : base(AoceWrapperPINVOKE.INobleCornerDetectionLayer_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(INobleCornerDetectionLayer obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          AoceWrapperPINVOKE.delete_INobleCornerDetectionLayer(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

  public void updateParamet(NobleCornerDetectionParamet t) {
    AoceWrapperPINVOKE.INobleCornerDetectionLayer_updateParamet(swigCPtr, NobleCornerDetectionParamet.getCPtr(t));
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
  }

  public NobleCornerDetectionParamet getParamet() {
    NobleCornerDetectionParamet ret = new NobleCornerDetectionParamet(AoceWrapperPINVOKE.INobleCornerDetectionLayer_getParamet(swigCPtr), true);
    return ret;
  }

}