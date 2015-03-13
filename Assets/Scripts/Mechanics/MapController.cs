﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TBTK;

public class MapController : MonoBehaviour {

	public Transform tilesPrefab;


	public static MapController instance;

	public float rangeTunel = 0.2f;
	
	private IList<Transform> generatedTileList;

	private float height;
	private float line;

	void Awake(){
		if (instance == null) {
			instance = this;
		}

		generatedTileList = new List<Transform> ();

		for (int a =0; a < 20; a++) {
			generatedTileList.Add(tilesPrefab);
		}

		line = GridGenerator.spaceXHex * GridManager.GetInstance().tileSize * GridManager.GetInstance().gridToTileRatio/1.5f;
		height = line * Mathf.Sin (Mathf.PI / 3);
		
	}


	//this method is called when it's necessary to instantiate a new area, given a tile
	public static void RevealArea(Tile tile){
		instance._RevealArea (tile);
	}

	private void _RevealArea(Tile tile){
//		IList<Tile> tileList = tile.GetNeighbourList ();

		int tileNumber = tile.tileNumber;

		//positioning tile according to it's tile number

		//distance for second int x = 2 * width
		//distance for first int x = Mathf.Sin (60f) * width * 4;

		GridManager gridInstance = GridManager.GetInstance ();

		Vector3 firstVector = instance.findFirstVector (tile);

		//-60*number - 90
		Vector3 secondVector = instance.findSecondVector (tile);

		Vector3 sourcePosition = tile.transform.position;

		Transform first = null;
		Transform second = null;
		switch (tile.revealed) {
			case 0:
				first = (Transform) Instantiate(tilesPrefab, sourcePosition + firstVector, new Quaternion(0,0,0,0));
				second = (Transform) Instantiate(tilesPrefab, sourcePosition + secondVector, new Quaternion(0,0,0,0));
				break;
			case 1:
				second = (Transform) Instantiate(tilesPrefab, sourcePosition + secondVector, new Quaternion(0,0,0,0));
				break;
			case 2:
				first = (Transform) Instantiate(tilesPrefab, sourcePosition + firstVector, new Quaternion(0,0,0,0));
				break;
		}

		Transform mainGridT= GameObject.FindGameObjectWithTag("MainGrid").transform;
		//this list will be used to logically add all tiles to their respective neighbours
		List<Tile> addedTiles = new List<Tile>();



		//add all new tiles to the gameObject hierarchy as well as add all of them to the existing tiles logic
		if (first != null) {
			initTiles (first, gridInstance, addedTiles, (line*2) + rangeTunel);
			addTiles (tile, addedTiles,(line*2) + rangeTunel);
			addedTiles = new List<Tile>();
		}

		if (second != null) {
			initTiles (second, gridInstance, addedTiles, (line*2) + rangeTunel);
			addTiles (tile, addedTiles,(line*2) + rangeTunel);
		}

		if(first != null)	GameObject.Destroy(first.gameObject);
		if(second != null) 	GameObject.Destroy(second.gameObject);


	}

	//Initialize all new tiles, so all their logic is ready to be appended to the existing tiles
	private void initTiles(Transform tilesT, GridManager instance, List<Tile> added, float range){
		List<Tile> tileList = new List<Tile> ();

		for (int a = tilesT.childCount - 1; a >=0; a--) {
			Tile tile = (Tile)tilesT.GetChild (a).GetComponent<Tile> ();
			tileList.Add(tile);
		}
		foreach(Tile tile in tileList){

			tile.aStar=new TileAStar(tile);
			
			List<Tile> neighbourList=new List<Tile>();

			foreach(Tile t in tileList){
				if(!t.Equals(tile) && Vector3.Distance(t.transform.position, tile.transform.position) <= range) neighbourList.Add(t);
			}
			
			tile.aStar.SetNeighbourList(neighbourList);

			instance.grid.tileList.Add(tile);

			added.Add(tile);
			if(neighbourList.Count == 6) tile.revealed = 3;
		}
	}
	
	//logically add all new tiles to the  existing tiles
	private void addTiles(Tile origin, List<Tile> tileList, float range){
		Tile currentTile = origin;

		List<Tile> openList = new List<Tile> ();
		List<Tile> closeList = new List<Tile> ();

		while (currentTile != null) {
			closeList.Add(currentTile);
			currentTile.aStar.listState = TileAStar._AStarListState.Close;
			List<Tile> newNeighbourList = currentTile.GetNeighbourList();

			int tileNumber = currentTile.tileNumber;

			Vector3 firstVector = instance.findFirstVector(currentTile);
			Vector3 secondVector = instance.findSecondVector(currentTile);

			bool first = false;
			bool second = false;



			foreach(Tile t in tileList){
//				if(t.Equals(currentTile)){Debug.Log(string.Format("T is {0} and currenTile is {1} and their respective positions are {2} and {3}", t, currentTile, t.transform.position, currentTile.transform.position));}
				Debug.Log (string.Format ("Tile {0} is being analyzed by tile {1}, and their distance is {2}, and the accepted range is {3}, and current tile position is {4}", t, currentTile, Vector3.Distance(currentTile.transform.position, t.transform.position), range, currentTile.transform.position)); 
				if(Vector3.Distance(currentTile.transform.position, t.transform.position) <=range && !newNeighbourList.Contains(t) ){
					List<Tile> newList = t.GetNeighbourList();
					newList.Add(currentTile);
					t.aStar.SetNeighbourList(newList);
					newNeighbourList.Add(t);


					//check if this would be the first or second new tile for currentTile, the bools will be checked afterwards to know what
					//should be done

					if(currentTile.name == "Tile_2" && Vector3.Distance(currentTile.transform.position, new Vector3(4.5f, 0, 2.6f)) <= 0.5f) Debug.Log (string.Format("Second vector is {0} and the difference is {1} and the difference between the expected and this is {2} which makes the bool {3}",secondVector, (t.transform.parent.position - currentTile.transform.position), Vector3.Distance(t.transform.parent.position - currentTile.transform.position, secondVector),t.transform.parent.position - currentTile.transform.position == secondVector));
					if(Vector3.Distance((t.transform.parent.position - currentTile.transform.position),firstVector) <= rangeTunel){
						first = true;

						//now we need to check what this guy represents for t
						Vector3 firstVectorForT = instance.findFirstVector(t);
						Vector3 secondVectorForT = instance.findSecondVector(t);

						bool distanceFromFirst = Vector3.Distance((currentTile.tile0.transform.position - t.transform.position), firstVectorForT) <= rangeTunel;
						bool distanceFromSecond = Vector3.Distance((currentTile.tile0.transform.position - t.transform.position), secondVectorForT) <= rangeTunel;
						updateTileRevealed(t, distanceFromFirst,  distanceFromSecond);
					}
					if(Vector3.Distance((t.transform.parent.position - currentTile.transform.position),secondVector) <= rangeTunel){
						if(currentTile.name == "Tile_2" && Vector3.Distance(currentTile.transform.position, new Vector3(4.5f, 0, 2.6f)) <= 0.5f) Debug.Log ("Got here");

						second = true;
						//now we need to check what this guy represents for t
						Vector3 firstVectorForT = instance.findFirstVector(t);
						Vector3 secondVectorForT = instance.findSecondVector(t);

						bool distanceFromFirst = Vector3.Distance((currentTile.tile0.transform.position - t.transform.position), firstVectorForT) <= rangeTunel;
						bool distanceFromSecond = Vector3.Distance((currentTile.tile0.transform.position - t.transform.position), secondVectorForT) <= rangeTunel;
						updateTileRevealed(t, distanceFromFirst,  distanceFromSecond);

					}
				}
			}


			Debug.Log (string.Format("Tile name is {0} and it's revealed number is {1} and it's position is {2} before it was updated", currentTile, currentTile.revealed, currentTile.transform.position));
			updateTileRevealed(currentTile, first, second);
			Debug.Log (string.Format("Tile name is {0} and it's revealed number is {1} and it's position is {2} after it was updated", currentTile, currentTile.revealed, currentTile.transform.position));
			Debug.Log ("---------------------------------------------------------------------------------------------------");
			if(first || second){
				foreach(Tile t in currentTile.GetNeighbourList()){
					if(t.aStar.listState != TileAStar._AStarListState.Close && t.revealed != 3){
						openList.Add(t);
					}
				}
			}

			currentTile.aStar.SetNeighbourList(newNeighbourList);

			currentTile = null;
			if(openList.Count != 0){
				currentTile = openList[0];
				openList.RemoveAt(0);
			}
		}

		foreach (Tile t in tileList) {
			t.transform.SetParent(GameObject.FindGameObjectWithTag ("MainGrid").transform);
		}
		
		AStar.ResetGraph (origin, openList, closeList);
		Debug.Log (origin.GetNeighbourList ().Count);
	}

	private void updateTileRevealed(Tile tile, bool first, bool second){
		if(tile.revealed == 0 && first) tile.revealed = 1;
		if(tile.revealed == 0 && second) tile.revealed = 2;
		else if(tile.revealed == 1 && second) tile.revealed = 3;
		else if(tile.revealed == 2 && first) tile.revealed =3;
		else if(first && second) tile.revealed = 3;
	}

	private Vector3 findFirstVector(Tile tile){
		
		Quaternion firstRotation = Quaternion.Euler (0, (-60 * (tile.tileNumber - 1)) - 90, 0);
		Vector3 firstVector = firstRotation * new Vector3 (1, 0, 0) * height * 4;	
		
		return firstVector;
	}
	
	private Vector3 findSecondVector(Tile tile){
		Quaternion secondRotation = Quaternion.Euler (0, (-60 * (tile.tileNumber - 1)), 0);
		Vector3 secondVector = secondRotation * new Vector3 (1, 0, 0) * line * 3;
		
		return secondVector;
	}
}