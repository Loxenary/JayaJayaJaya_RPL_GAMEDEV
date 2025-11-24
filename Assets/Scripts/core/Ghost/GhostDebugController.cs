using UnityEngine;

/// <summary>
/// Helper script untuk testing ghost system di editor
/// Attach ke ghost GameObject untuk control manual
/// </summary>
public class GhostDebugController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GhostAI ghostAI;
    [SerializeField] private GhostAttack ghostAttack;
    [SerializeField] private PlayerSanity playerSanity;
    
    [Header("Debug Controls")]
    [SerializeField] private KeyCode toggleActiveKey = KeyCode.G;
    [SerializeField] private KeyCode stunKey = KeyCode.H;
    [SerializeField] private KeyCode teleportToPlayerKey = KeyCode.J;
    [SerializeField] private KeyCode reduceSanityKey = KeyCode.K;
    [SerializeField] private KeyCode increaseSanityKey = KeyCode.L;
    
    [Header("Debug Settings")]
    [SerializeField] private float stunDuration = 3f;
    [SerializeField] private float sanityChangeAmount = 10f;
    
    [Header("Debug Display")]
    [SerializeField] private bool showDebugGUI = true;
    [SerializeField] private Vector2 guiPosition = new Vector2(10, 300);
    
    private GameObject player;
    
    private void Start()
    {
        if (ghostAI == null)
            ghostAI = GetComponent<GhostAI>();
        
        if (ghostAttack == null)
            ghostAttack = GetComponent<GhostAttack>();
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && playerSanity == null)
        {
            playerSanity = player.GetComponent<PlayerSanity>();
        }
    }
    
    private void Update()
    {
        HandleDebugInput();
    }
    
    private void HandleDebugInput()
    {
        // Toggle ghost active/inactive
        if (Input.GetKeyDown(toggleActiveKey))
        {
            if (ghostAI != null)
            {
                ghostAI.SetActive(!ghostAI.IsActive);
                Debug.Log($"[GhostDebug] Ghost active: {ghostAI.IsActive}");
            }
        }
        
        // Stun ghost
        if (Input.GetKeyDown(stunKey))
        {
            if (ghostAI != null)
            {
                ghostAI.Stun(stunDuration);
                Debug.Log($"[GhostDebug] Ghost stunned for {stunDuration}s");
            }
        }
        
        // Teleport to player
        if (Input.GetKeyDown(teleportToPlayerKey))
        {
            if (ghostAI != null && player != null)
            {
                ghostAI.Teleport(player.transform.position + Vector3.back * 5f);
                Debug.Log("[GhostDebug] Ghost teleported to player");
            }
        }
        
        // Reduce sanity
        if (Input.GetKey(reduceSanityKey))
        {
            if (playerSanity != null)
            {
                playerSanity.DecreaseSanity(sanityChangeAmount * Time.deltaTime);
            }
        }
        
        // Increase sanity
        if (Input.GetKey(increaseSanityKey))
        {
            if (playerSanity != null)
            {
                playerSanity.IncreaseSanity(sanityChangeAmount * Time.deltaTime);
            }
        }
    }
    
    private void OnGUI()
    {
        if (!showDebugGUI) return;
        
        float x = guiPosition.x;
        float y = guiPosition.y;
        float lineHeight = 20f;
        int line = 0;
        
        GUI.Box(new Rect(x, y, 300, 200), "Ghost Debug Panel");
        y += 25;
        
        // Ghost State
        if (ghostAI != null)
        {
            GUI.Label(new Rect(x + 10, y + (line++ * lineHeight), 280, lineHeight), 
                $"State: {ghostAI.CurrentState}");
            GUI.Label(new Rect(x + 10, y + (line++ * lineHeight), 280, lineHeight), 
                $"Speed: {ghostAI.CurrentSpeed:F2}");
            GUI.Label(new Rect(x + 10, y + (line++ * lineHeight), 280, lineHeight), 
                $"Active: {ghostAI.IsActive}");
        }
        
        // Attack State
        if (ghostAttack != null)
        {
            GUI.Label(new Rect(x + 10, y + (line++ * lineHeight), 280, lineHeight), 
                $"Can Attack: {ghostAttack.CanAttack()}");
            GUI.Label(new Rect(x + 10, y + (line++ * lineHeight), 280, lineHeight), 
                $"Attacking: {ghostAttack.IsAttacking}");
        }
        
        // Player Sanity
        if (playerSanity != null)
        {
            GUI.Label(new Rect(x + 10, y + (line++ * lineHeight), 280, lineHeight), 
                $"Sanity: {playerSanity.CurrentSanity:F1}/{playerSanity.MaxSanity}");
            GUI.Label(new Rect(x + 10, y + (line++ * lineHeight), 280, lineHeight), 
                $"Level: {playerSanity.CurrentSanityLevel}");
        }
        
        // Controls
        y += (line * lineHeight) + 10;
        GUI.Label(new Rect(x + 10, y, 280, lineHeight), 
            $"Controls: [{toggleActiveKey}] Active [{stunKey}] Stun");
        y += lineHeight;
        GUI.Label(new Rect(x + 10, y, 280, lineHeight), 
            $"[{teleportToPlayerKey}] TP [{reduceSanityKey}/{increaseSanityKey}] Sanity");
    }
}
