// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using UnityEngine;
using System;
using TBTK;

namespace Cards
{
	public class CardTransform : MonoBehaviour
	{
		public Card transformCard;

		public Vector3 endScale;
		public static float holdTimeTreshold = 0.2f;
		public Vector3 finalPosition;

		private Vector3 initialPosition;
		private Vector3 baseScale;
		private float holdTime;

		private bool cardHeld = false;

		private bool zoomed;

		void Start(){
			this.transformCard = (Card) transform.GetComponent<Card> ();
		}

		void Update(){
			if (cardHeld) {
				holdTime += Time.deltaTime;
				if (holdTime >= holdTimeTreshold && !zoomed) {

					CardsHandManager.DeselectCard();

					zoomed = true;
					ZoomCard ();
				}
			} else if(zoomed) {
				DeZoom();
				zoomed = false;
			}
		}


		private void activateCard(CardsStackManager stack){
			if (this.transformCard.damageCard) 
				stack.addDamageCard(this.transformCard);
			if (this.transformCard.guardCard)
				stack.addGuardCard (this.transformCard);
			if (this.transformCard.moveCard)
				stack.addMoveCard (this.transformCard);

			GameControl.SelectUnit (GameControl.selectedUnit);

			//Do some sort of animation then destroy this card
			this.transform.SetParent (null);
			this.transform.position = CardsHandManager.getInstance ().cardsLimbo;
			CardsHandManager.getInstance ().cardsInDiscard.addCard (transformCard);
		}

		public void selectCard(){
		
		}

		public void deselectCard(){
			
		}

		public void ZoomCard(){
			this.baseScale = transform.localScale;
			this.initialPosition = transform.position;
			transformCard.updateTransform (transform.position + finalPosition, this.transform.rotation, endScale);
		}
		public void DeZoom(){
			transformCard.updateTransform (initialPosition, this.transform.rotation, baseScale);
		}

		void OnMouseDown(){
			if (CardsHandManager.getInstance () != null && CardsHandManager.getInstance ().mode == CardsHandManager.modes._DeckBuild) {
				GameObject o = (GameObject)Resources.Load ("Prefabs/Cards/" + this.transform.name);
				CardsHandManager.getInstance ().instantiator.addPrefab (o);
			} else if (CardsHandManager.getInstance () != null && CardsHandManager.instance.selectedCard == this && GameControl.selectedUnit.factionID == FactionManager.GetPlayerFactionID () [0])
				activateCard ((CardsStackManager)GameControl.selectedUnit.GetComponent<CardsStackManager> ());
			else {
				cardHeld = true;
			}
		}

		void OnMouseUp(){
			if(CardsHandManager.getInstance () != null && CardsHandManager.getInstance ().mode == CardsHandManager.modes._GameOn && cardHeld){
				CardsHandManager.DeselectCard();
				selectCard();
			}
			cardHeld = false;
		}

	}
}

