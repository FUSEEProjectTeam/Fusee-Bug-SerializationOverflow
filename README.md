# SerializationOverflow

This example should throw the following exception:

```
Exception has occurred: CLR/System.OverflowException
An unhandled exception of type 'System.OverflowException' occurred in protobuf-net.dll: 'Arithmetic operation resulted in an overflow.'
   at ProtoBuf.ProtoReader.ReadUInt16()
   at Fusee.Serialization.Serializer.ReadMesh(Mesh , ProtoReader )
   at Fusee.Serialization.Serializer.ReadSceneComponentContainer(SceneComponentContainer , ProtoReader )
   at ProtoBuf.ProtoReader.ReadTypedObject(Object value, Int32 key, ProtoReader reader, Type type)
   at ProtoBuf.BclHelpers.ReadNetObject(Object value, ProtoReader source, Int32 key, Type type, NetObjectOptions options)
   at Fusee.Serialization.Serializer.ReadSceneNodeContainer(SceneNodeContainer , ProtoReader )
   at ProtoBuf.ProtoReader.ReadTypedObject(Object value, Int32 key, ProtoReader reader, Type type)
   at ProtoBuf.BclHelpers.ReadNetObject(Object value, ProtoReader source, Int32 key, Type type, NetObjectOptions options)
   at Fusee.Serialization.Serializer.ReadSceneContainer(SceneContainer , ProtoReader )
   at ProtoBuf.Meta.TypeModel.DeserializeCore(ProtoReader reader, Type type, Object value, Boolean noAutoCreate)
   at ProtoBuf.Meta.TypeModel.Deserialize(Stream source, Object value, Type type, SerializationContext context)
   at ProtoBuf.Meta.TypeModel.Deserialize(Stream source, Object value, Type type)
   at Fusee.Engine.Player.Desktop.Simple.<>c.<Main>b__1_4(String id, Object storage)
   at Fusee.Base.Common.StreamAssetProvider.GetAsset(String id, Type type)
   at Fusee.Base.Core.AssetStorage.GetAsset[T](String id)
   at Fusee.Base.Core.AssetStorage.Get[T](String id)
   at FuseeApp.SerializationOverflow.Init() in E:\Git\Fusee-Bug-SerializationOverflow\SerializationOverflow.cs:line 67
   at Fusee.Engine.Core.RenderCanvas.<InitCanvas>b__31_0(Object <p0>, InitEventArgs <p1>)
   at Fusee.Engine.Imp.Graphics.Desktop.RenderCanvasImpBase.DoInit()
   at Fusee.Engine.Imp.Graphics.Desktop.RenderCanvasGameWindow.OnLoad(EventArgs e)
   at OpenTK.GameWindow.Run(Double updates_per_second, Double frames_per_second)
   at Fusee.Engine.Imp.Graphics.Desktop.RenderCanvasImp.Run()
   at Fusee.Engine.Core.RenderCanvas.Run()
   at Fusee.Engine.Player.Desktop.Simple.Main(String[] args)
```

This is because of the model size, the same model with a reduced amount of geometry works (terrain_smaller.fus is included and works). The source objs for the model is in the "RAW" folder.

terrain.obj -> Overflow
* 15.099 Vertices
* 29.999 Faces

terrain_smaller.obj -> Works
* 7.582 Vertices
* 14.998 Faces