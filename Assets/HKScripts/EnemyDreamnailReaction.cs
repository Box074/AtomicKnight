using UnityEngine;

public class EnemyDreamnailReaction : MonoBehaviour
{
	protected void Reset()
	{
		this.convoAmount = 8;
		this.convoTitle = "GENERIC";
		this.startSuppressed = false;
	}


	private const int RegularMPGain = 33;

	private const int BoostedMPGain = 66;

	private const float AttackMagnitude = 2f;

	private const float CooldownDuration = 0.2f;

	[SerializeField]
	private int convoAmount;

	[SerializeField]
	private string convoTitle;

	[SerializeField]
	private bool startSuppressed;

	[SerializeField]
	private bool noSoul;

	[SerializeField]
	private GameObject dreamImpactPrefab;

	public bool allowUseChildColliders;

	private EnemyDreamnailReaction.States state;

	private float cooldownTimeRemaining;

	private enum States
	{
		Suppressed,
		Ready,
		CoolingDown
	}
}
