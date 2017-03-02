﻿using System;
using System.Collections.Generic;
using UnityEngine;
using g3;

namespace f3
{
    public static class UnityExtensions
    {
        public static void SetName(this GameObject go, string name) {
            go.name = name;
        }
        public static string GetName(this GameObject go) {
            return go.name;
        }

        public static void SetLayer(this GameObject go, int layer) {
            go.layer = layer;
        }
        public static int GetLayer(this GameObject go) {
            return go.layer;
        }

        public static bool HasChildren(this GameObject go) {
            return go.transform.childCount > 0;
        }
        public static IEnumerable<GameObject> Children(this GameObject go) {
            for (int k = 0; k < go.transform.childCount; ++k)
                yield return go.transform.GetChild(k).gameObject;
        }

        public static void AddChild(this GameObject go, GameObject child, bool bKeepWorldPosition)
        {
            child.transform.SetParent(go.transform, bKeepWorldPosition);
        }
        public static void RemoveChild(this GameObject go, GameObject child)
        {
            child.transform.SetParent(null);
        }
        public static void RemoveFromParent(this GameObject go)
        {
            go.transform.SetParent(null);
        }


        public static Mesh GetMesh(this GameObject go)
        {
            return go.GetComponent<MeshFilter>().mesh;
        }
        public static Mesh GetSharedMesh(this GameObject go)
        {
            return go.GetComponent<MeshFilter>().sharedMesh;
        }
        public static void SetMesh(this GameObject go, Mesh m, bool bUpdateCollider = false) {
            go.GetComponent<MeshFilter>().mesh = m;
            if ( bUpdateCollider ) {
                MeshCollider c = go.GetComponent<MeshCollider>();
                if (c != null)
                    c.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
            }
        }
        public static void SetSharedMesh(this GameObject go, Mesh m, bool bUpdateCollider = false) {
            go.GetComponent<MeshFilter>().sharedMesh = m;
            if ( bUpdateCollider ) {
                MeshCollider c = go.GetComponent<MeshCollider>();
                if (c != null)
                    c.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
            }
        }

        public static void SetMaterial(this GameObject go, fMaterial mat, bool bShared = false) {
            if ( bShared )
                go.GetComponent<Renderer>().sharedMaterial = mat;
            else
                go.GetComponent<Renderer>().material = mat;
        }
        public static fMaterial GetMaterial(this GameObject go) {
            Renderer r = go.GetComponent<Renderer>();
            return (r != null) ? new fMaterial(r.material) : null;
        }



        public static void Hide(this GameObject go)
        {
            if (go.activeSelf == true)
                go.SetActive(false);
        }
        public static void Show(this GameObject go)
        {
            if (go.activeSelf == false)
                go.SetActive(true);
        }
        public static void SetVisible(this GameObject go, bool bVisible)
        {
            if (bVisible) Show(go); else Hide(go);
        }
        public static bool IsVisible(this GameObject go)
        {
            return (go.activeSelf == true);
        }



        public static Vector3f GetLocalScale(this GameObject go)
        {
            return go.transform.localScale;
        }
        public static Vector3f GetLocalPosition(this GameObject go)
        {
            return go.transform.localPosition;
        }




        /*
         * Camera extensions
         */


        public static void SetName(this Camera cam, string name) {
            cam.name = name;
        }
        public static string GetName(this Camera cam) {
            return cam.name;
        }



        /*
         * Material extensions
         */


        public static void SetName(this Material mat, string name) {
            mat.name = name;
        }
        public static string GetName(this Material mat) {
            return mat.name;
        }




    }

}