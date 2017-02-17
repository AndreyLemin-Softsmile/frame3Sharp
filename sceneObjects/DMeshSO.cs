﻿using System;
using System.Collections.Generic;
using g3;

using UnityEngine;

namespace f3
{


    public class DMeshSO : BaseSO, IMeshComponentManager
    {
        protected fGameObject parentGO;
        protected List<fMeshGameObject> displayGOs;

        DMesh3 mesh;
        MeshDecomposition decomp;


        bool enable_spatial = true;
        DMeshAABBTree3 spatial;


		public DMeshSO()
		{
		}

        public virtual DMeshSO Create(DMesh3 mesh, SOMaterial setMaterial)
        {
            AssignSOMaterial(setMaterial);       // need to do this to setup BaseSO material stack
            parentGO = GameObjectFactory.CreateParentGO(UniqueNames.GetNext("DMesh"));

            this.mesh = mesh;
            on_mesh_changed();

            displayGOs = new List<fMeshGameObject>();
            validate_decomp();

            return this;
        }


        // Currently do not support changing mesh after creation!!
        public DMesh3 Mesh
        {
            get { return mesh; }
        }




        public DMeshAABBTree3 Spatial
        {
            get { validate_spatial(); return spatial; }
        }
        public bool EnableSpatial
        {
            get { return enable_spatial; }
            set { enable_spatial = value; }
        }





        public void ReplaceMesh(DMesh3 newMesh)
        {
            this.mesh = newMesh;

            on_mesh_changed();
            validate_decomp();
        }


        public void UpdateVertexPositions(Vector3f[] vPositions)
        {
            if (vPositions.Length != mesh.MaxVertexID)
                throw new Exception("DMeshSO.UpdateVertexPositions: not enough positions provided!");

            foreach ( int vid in mesh.VertexIndices() ) {
                mesh.SetVertex(vid, vPositions[vid]);
            }

            // [TODO] fast spatial decomposition update (we can keep structure,
            //   just need to update mesh vertices and collider stuff...)
            on_mesh_changed();
            validate_decomp();
        }



        #region IMeshComponentManager impl

        public void AddComponent(MeshDecomposition.Component C)
        {
            fMesh submesh = new fMesh(C.triangles, mesh, C.source_vertices, true, true);
            fMeshGameObject go = GameObjectFactory.CreateMeshGO("component", submesh, false);
            go.SetMaterial( new fMaterial(CurrentMaterial) );
            displayGOs.Add(go);

            AppendNewGO(go, parentGO, false);
        }

        public void ClearAllComponents()
        {
            foreach (fMeshGameObject go in displayGOs) {
                RemoveGO(go);
                go.Destroy();
            }
            displayGOs = new List<fMeshGameObject>();
        }

        #endregion





        //
        // internals
        // 
        void on_mesh_changed()
        {
            spatial = null;
            decomp = null;

            // discard existing mesh GOs
            if (displayGOs != null) {
                foreach (fMeshGameObject go in displayGOs) {
                    RemoveGO(go);
                    go.Destroy();
                }
            }
            displayGOs = new List<fMeshGameObject>();
        }

        void validate_spatial()
        {
            if ( enable_spatial && spatial == null ) {
                spatial = new DMeshAABBTree3(mesh);
                spatial.Build();
            }
        }

        void validate_decomp()
        {
            if ( decomp == null ) {
                decomp = new MeshDecomposition(mesh, this);
                decomp.BuildLinear();
            }
        }



        //
        // SceneObject impl
        //
        override public GameObject RootGameObject
        {
            get { return parentGO; }
        }

        override public string Name
        {
            get { return parentGO.GetName(); }
            set { parentGO.SetName(value); }
        }

        override public SOType Type { get { return SOTypes.DMesh; } }

        public override bool IsSurface {
            get { return true; }
        }

        override public SceneObject Duplicate()
        {
            DMeshSO copy = new DMeshSO();
            DMesh3 copyMesh = new DMesh3(mesh);
            copy.Create( copyMesh, this.GetAssignedSOMaterial() );
            copy.SetLocalFrame(
                this.GetLocalFrame(CoordSpace.ObjectCoords), CoordSpace.ObjectCoords);
            copy.SetLocalScale(this.GetLocalScale());
            return copy;
        }

        override public AxisAlignedBox3f GetLocalBoundingBox()
        {
            AxisAlignedBox3f b = (AxisAlignedBox3f)mesh.CachedBounds;
            Vector3f scale = parentGO.GetLocalScale();
            b.Scale(scale.x, scale.y, scale.z);
            return b;
        }


        override public void DisableShadows() {
            //MaterialUtil.DisableShadows(meshGO, true, false);
        }



        // [RMS] this is not working right now...
        override public bool FindRayIntersection(Ray3f ray, out SORayHit hit)
        {
            if (spatial == null) {
                spatial = new DMeshAABBTree3(mesh);
                spatial.Build();
            }

            Transform xform = ((GameObject)RootGameObject).transform;

            // convert ray to local
            Ray3d local_ray = new Ray3d();
            local_ray.Origin = xform.InverseTransformPoint(ray.Origin);
            local_ray.Direction = xform.InverseTransformDirection(ray.Direction);
            local_ray.Direction.Normalize();

            hit = null;
            int hit_tid = spatial.FindNearestHitTriangle(local_ray);
            if (hit_tid != DMesh3.InvalidID) {
                IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(mesh, hit_tid, local_ray);

                hit = new SORayHit();
                hit.fHitDist = (float)intr.RayParameter;
                hit.hitPos = xform.TransformPoint((Vector3f)local_ray.PointAt(intr.RayParameter));
                hit.hitNormal = xform.TransformDirection((Vector3f)mesh.GetTriNormal(hit_tid));
                hit.hitGO = RootGameObject;
                hit.hitSO = this;
                return true;
            }
            return false;
        }


    }
}
