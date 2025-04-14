Skinn: Vertex Mapper Copyright (c) 2019 All Rights Reserved. cwmanley@gmail.com  http://www.unity3d.com/legal/as_terms 

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

Special Thanks to the Unity and UMA communities.

Skinn Vertex Mapper (Free Edition)

A free fully featured standalone version of the Vertex Mapper.

!!! Remove previous installation before installing
!!! Model Import Settings; Read Write Enable = true, Mesh Compression = Off, Keep Quads = false
!!! A platform that supports compute shaders is required!

Videos https://www.youtube.com/channel/UCBSInjON0ldeK4XzSZcMPTg
________________________________________________________________________

***Vertex Mapper***
_________________________________________________________________________________________

The Vertex Mapper is a component that creates a skinned mesh from a target renderer with the skin and or blendshapes from a source mesh.

There are tool-tips in the component that explain the settings for the each of the sections.
Context-Clicking the Foldout(title) of a section opens a menu for restoring the default values and apply any built in presets.
Additionally you can reset all the sections with the ContextMenu of the VM component.

VM_ Source Asset

- the source of the skin and blend-shapes.
- can be any prefab as long as it contains a skinned mesh renderer that is setup up correctly.
- should be in a T-Pose, bind-poses are created from the source pose.

VM_ Target Asset

- the source of the skin and blend-shapes.
- can be any prefab as long as it contains a renderer with a mesh.
- should be transformed or posed with bones to fit the source.

VM_ Root

- root of the created skinned mesh.
- can be null
- must be in a scene.
- cannot be the source game-object or the target.
- can be in any location or pose.
- if a child matches a merged bone it is used; otherwise a new game-object is created, parented and used as a bone.
- new bones cannot be parents of old bones.

VM_ Sections

Baking - control the vertex space for the sources and target assets.

Search - compare distance, normals, tangents, uvs, submeshes. if no source vertices fall with-in the search settings the setting are ignored.

Shape Projection - multi-frame support, calculate normals and tangents, inflate, radial blending.
 
Weight Projection - surface weight, radial, adjacent, delta mush blending options. 

Gizmos - source, target, mapping, and search distance are displayed with the baking settings in real-time.

Bone Filter - filter individual bones from the search.

Shape Filter - filter individual blend-shapes from projection.

Create - a mesh from the target asset and the skin of the source asset will be created, then the skeleton will be merged on to the Root.

Append - remove the last created asset from the root and create a new skeleton with the same mesh.


VM_ Saving Assets.

Unity Multipurpose Avatar System - Once you are done with the VM preview asset Append and use UMA Slot Builder like normal.

Context Command  - [MenuItem("CONTEXT/SkinnedMeshRenderer/Skinn VM: Save Mesh")]

________________________________________________________________________

***Issues***
_________________________________________________________________________________________

Skinned meshes with blend-shapes but no bone-weights can not be used as a target renderer with the Vertex Mapper.
Solution use a Mesh Renderer with the same mesh.

________________________________________________________________________

***More***
_________________________________________________________________________________________

For the latest version of the Vertex Mapper and addition features you can purchase Skinn.
Skinn saves mesh assets with prefabs, exports fbx, drags and drops skinned meshes into a single models and much more!

Skinn on the Unity Asset Store https://www.assetstore.unity3d.com/en/#!/content/86532
Skinn on the itch.io Store https://cwmanley.itch.io/skinn-vertex-mapper
