//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class ImageFormat : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal ImageFormat(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(ImageFormat obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~ImageFormat() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    global::System.GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          AoceWrapperPINVOKE.delete_ImageFormat(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public int width {
    set {
      AoceWrapperPINVOKE.ImageFormat_width_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.ImageFormat_width_get(swigCPtr);
      return ret;
    } 
  }

  public int height {
    set {
      AoceWrapperPINVOKE.ImageFormat_height_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.ImageFormat_height_get(swigCPtr);
      return ret;
    } 
  }

  public ImageType imageType {
    set {
      AoceWrapperPINVOKE.ImageFormat_imageType_set(swigCPtr, (int)value);
    } 
    get {
      ImageType ret = (ImageType)AoceWrapperPINVOKE.ImageFormat_imageType_get(swigCPtr);
      return ret;
    } 
  }

  public ImageFormat() : this(AoceWrapperPINVOKE.new_ImageFormat(), true) {
  }

}
