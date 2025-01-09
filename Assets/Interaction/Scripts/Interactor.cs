using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    private IInteractable interactable = null;
    public void Update()
    {
        Ray r = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(r, out RaycastHit hitInfo, 3f))
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactableObj))
            {

                if (interactable != interactableObj)
                {
                    if (interactable != null) { interactable.OnExit(); };
                    interactableObj.OnEnter();
                    interactable = interactableObj;
                }

                if (interactable != null)
                {
                    interactableObj.OnHover();
                }
                //Debug.Log("Hovered");


            }
            else
            {
                if (interactable != null)
                {
                    interactable.OnExit();
                    interactable = null;
                }
                else
                {
                    interactable = null;
                }
            }

        }
        else
        {
            if (interactable != null)
            {
                interactable.OnExit();
                interactable = null;
            }
            else
            {
                interactable = null;
            }
        }
    }
}
