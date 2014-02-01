//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class VoxelWorld : MonoBehaviour
{
		public GameObject regionPrefab;
		public int regionXZ = 32;
		public int regionY = 32;
		private Dictionary<string, Region> regions;
		private LinkedList<Region> dirtyRegions;
		private VoxelModifyTerrain clientRenderer;
		public bool useDisk = false;

		void Start ()
		{
				Debug.Log (Application.persistentDataPath);
				Region.setWorld (this);
				regions = new Dictionary<string, Region> ();
				print ("Creating client region");
				Region centerRegion = createRegion (0, 0, false);
				print ("Loading client's neighbor regions.");
				loadAllNeighbors (centerRegion, false);

				print ("Creating client renderer");
				clientRenderer = gameObject.GetComponent ("VoxelModifyTerrain") as VoxelModifyTerrain;
				clientRenderer.setStartRegion (centerRegion);


				InvokeRepeating ("SaveToDiskEvent", 30f, 30f);
		}

		public void changeFocusRegion (Region newRegion)
		{
				//Load up region neighbors. Create a new regions as needed.
				loadAllNeighbors (newRegion, true);  


		}

		/**
	 * Gets the region at the specified indices in the Dictionary
	 * 
	 **/
		public Region getRegionAtIndex (int x, int z)
		{
				string key = x + "x" + z;
				if (regions.ContainsKey (key)) {
						Region region = regions [key];
						return region;
				} else {
						return null;
				}

		}

		/**
	 * Gets the region containing the specified world cordinates
	 * 
	 **/
		public Region getRegionAtCoords (int x, int z)
		{
				if (x < 0) {
						x = x - regionXZ + 1;
				}
				if (z < 0) {
						z = z - regionXZ + 1;
				}
			
				int regionsX = x / regionXZ;
				int regionsZ = z / regionXZ;
		
				string key = regionsX + "x" + regionsZ;
				if (regions.ContainsKey (key)) {
						Region region = regions [key];
						return region;
				} else {
						Debug.LogError ("Region [" + key + "] not found in Dictionary!");
						Application.Quit ();
						return null;
				}
		
		}

		private void loadAllNeighbors (Region region, bool isAsync)
		{

				//North
				Region neighbor = getRegionAtIndex (region.offsetX, region.offsetZ + 1);
				if (neighbor == null) {
						neighbor = createRegion (region.offsetX, region.offsetZ + 1, isAsync);
				}


				//South
				neighbor = getRegionAtIndex (region.offsetX, region.offsetZ - 1);
				if (neighbor == null) {
						neighbor = createRegion (region.offsetX, region.offsetZ - 1, isAsync);
				}

				//East
				neighbor = getRegionAtIndex (region.offsetX + 1, region.offsetZ);
				if (neighbor == null) {
						neighbor = createRegion (region.offsetX + 1, region.offsetZ, isAsync);
				}

				//West
				neighbor = getRegionAtIndex (region.offsetX - 1, region.offsetZ);
				if (neighbor == null) {
						neighbor = createRegion (region.offsetX - 1, region.offsetZ, isAsync);
				}

				//NorthWest
				neighbor = getRegionAtIndex (region.offsetX - 1, region.offsetZ + 1);
				if (neighbor == null) {
						neighbor = createRegion (region.offsetX - 1, region.offsetZ + 1, isAsync);
				}

				//SouthEast
				neighbor = getRegionAtIndex (region.offsetX + 1, region.offsetZ - 1);
				if (neighbor == null) {
						neighbor = createRegion (region.offsetX + 1, region.offsetZ - 1, isAsync);
				}

				//NorthEast
				neighbor = getRegionAtIndex (region.offsetX + 1, region.offsetZ + 1);
				if (neighbor == null) {
						neighbor = createRegion (region.offsetX + 1, region.offsetZ + 1, isAsync);
				}

				//SouthWest
				neighbor = getRegionAtIndex (region.offsetX - 1, region.offsetZ - 1);
				if (neighbor == null) {
						neighbor = createRegion (region.offsetX - 1, region.offsetZ - 1, isAsync);
				}

		}

		public void saveWorld ()
		{
				foreach (KeyValuePair<string,Region> kvp in regions) {
						kvp.Value.saveRegion ();
				}
		}

		private Region createRegion (int x, int z, bool isAsync)
		{
				GameObject regionGO = Instantiate (regionPrefab, new Vector3 (x * regionXZ, 0, z * regionXZ), new Quaternion (0, 0, 0, 0)) as GameObject;
				regionGO.transform.parent = this.transform;
				Region region = regionGO.GetComponent ("Region") as Region;
				region.regionXZ = this.regionXZ;
				region.regionY = this.regionY;
				region.regionXZ = this.regionXZ;
				region.offsetX = x;
				region.offsetZ = z;
				regions.Add (region.hashString (), region);
				if (isAsync) {
						Thread oThread = new Thread (new ThreadStart (region.create));
						oThread.Start ();
				} else {
						region.create ();
				}
				
				
				return region;
		}

		private void SaveToDiskEvent ()
		{
				if (useDisk) {
						foreach (KeyValuePair<string,Region> kvp in regions) {
								//TODO: Possible ConcurrentModification here.
								if (kvp.Value.isDirty) {
										Thread oThread = new Thread (new ThreadStart (kvp.Value.saveRegion));
										oThread.Start ();
								}
						}
				}
			
		}

}