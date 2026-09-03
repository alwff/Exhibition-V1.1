using UnityEngine;
using SUPERCharacter;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject prompt;

    [Header("Player")]
    public SUPERCharacterAIO playerController;

    private void Start()
    {
        if (prompt != null)
            prompt.SetActive(false);
    }

    private void Update()
    {
        if (prompt == null || playerController == null)
            return;

        // Si otro sistema tiene bloqueado al jugador el prompt no debe aparecer.
        if (InputBlocker.blockInput)
        {
            prompt.SetActive(false);
            return;
        }

        bool canInteract = IsLookingAtSpecimen();

        if (prompt.activeSelf != canInteract)
            prompt.SetActive(canInteract);
    }

    private bool IsLookingAtSpecimen()
    {
        Camera playerCamera = playerController.playerCamera;

        if (playerCamera == null)
            return false;

        RaycastHit hit;

        bool hitSomething = Physics.SphereCast(
            playerCamera.transform.position,
            0.25f,
            playerCamera.transform.forward,
            out hit,
            playerController.interactRange,
            playerController.interactableLayer,
            QueryTriggerInteraction.Ignore
        );

        if (!hitSomething)
            return false;

        PaintingSlot slot = hit.collider.GetComponent<PaintingSlot>();

        if (slot == null)
            return false;

        return slot.CanInteract();
    }
}