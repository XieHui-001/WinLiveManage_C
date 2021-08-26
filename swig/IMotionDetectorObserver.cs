//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class IMotionDetectorObserver : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal IMotionDetectorObserver(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(IMotionDetectorObserver obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~IMotionDetectorObserver() {
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
          AoceWrapperPINVOKE.delete_IMotionDetectorObserver(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public virtual void onMotion(vec4 vec) {
    AoceWrapperPINVOKE.IMotionDetectorObserver_onMotion(swigCPtr, vec4.getCPtr(vec));
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
  }

  public IMotionDetectorObserver() : this(AoceWrapperPINVOKE.new_IMotionDetectorObserver(), true) {
    SwigDirectorConnect();
  }

  private void SwigDirectorConnect() {
    if (SwigDerivedClassHasMethod("onMotion", swigMethodTypes0))
      swigDelegate0 = new SwigDelegateIMotionDetectorObserver_0(SwigDirectorMethodonMotion);
    AoceWrapperPINVOKE.IMotionDetectorObserver_director_connect(swigCPtr, swigDelegate0);
  }

  private bool SwigDerivedClassHasMethod(string methodName, global::System.Type[] methodTypes) {
    global::System.Reflection.MethodInfo methodInfo = this.GetType().GetMethod(methodName, global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Instance, null, methodTypes, null);
    bool hasDerivedMethod = methodInfo.DeclaringType.IsSubclassOf(typeof(IMotionDetectorObserver));
    return hasDerivedMethod;
  }

  private void SwigDirectorMethodonMotion(global::System.IntPtr vec) {
    onMotion(new vec4(vec, false));
  }

  public delegate void SwigDelegateIMotionDetectorObserver_0(global::System.IntPtr vec);

  private SwigDelegateIMotionDetectorObserver_0 swigDelegate0;

  private static global::System.Type[] swigMethodTypes0 = new global::System.Type[] { typeof(vec4) };
}