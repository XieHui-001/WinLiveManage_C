//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class IInputLayer : AInputLayer {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal IInputLayer(global::System.IntPtr cPtr, bool cMemoryOwn) : base(AoceWrapperPINVOKE.IInputLayer_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(IInputLayer obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          AoceWrapperPINVOKE.delete_IInputLayer(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

  public virtual void setImage(ImageFormat newFormat) {
    AoceWrapperPINVOKE.IInputLayer_setImage__SWIG_0(swigCPtr, ImageFormat.getCPtr(newFormat));
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
  }

  public virtual void setImage(VideoFormat newFormat) {
    AoceWrapperPINVOKE.IInputLayer_setImage__SWIG_1(swigCPtr, VideoFormat.getCPtr(newFormat));
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
  }

  public virtual void inputCpuData(global::System.IntPtr data, bool bSeparateRun) {
    AoceWrapperPINVOKE.IInputLayer_inputCpuData__SWIG_0(swigCPtr, data, bSeparateRun);
  }

  public virtual void inputCpuData(global::System.IntPtr data) {
    AoceWrapperPINVOKE.IInputLayer_inputCpuData__SWIG_1(swigCPtr, data);
  }

  public virtual void inputCpuData(VideoFrame videoFrame, bool bSeparateRun) {
    AoceWrapperPINVOKE.IInputLayer_inputCpuData__SWIG_2(swigCPtr, VideoFrame.getCPtr(videoFrame), bSeparateRun);
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
  }

  public virtual void inputCpuData(VideoFrame videoFrame) {
    AoceWrapperPINVOKE.IInputLayer_inputCpuData__SWIG_3(swigCPtr, VideoFrame.getCPtr(videoFrame));
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
  }

  public virtual void inputCpuData(global::System.IntPtr data, ImageFormat imageFormat, bool bSeparateRun) {
    AoceWrapperPINVOKE.IInputLayer_inputCpuData__SWIG_4(swigCPtr, data, ImageFormat.getCPtr(imageFormat), bSeparateRun);
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
  }

  public virtual void inputCpuData(global::System.IntPtr data, ImageFormat imageFormat) {
    AoceWrapperPINVOKE.IInputLayer_inputCpuData__SWIG_5(swigCPtr, data, ImageFormat.getCPtr(imageFormat));
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
  }

  public virtual void inputGpuData(global::System.IntPtr device, global::System.IntPtr tex) {
    AoceWrapperPINVOKE.IInputLayer_inputGpuData(swigCPtr, device, tex);
  }

}
