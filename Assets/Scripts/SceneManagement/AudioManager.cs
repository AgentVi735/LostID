using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    [Header("Sounds")]
    [SerializeField] private AudioClip trainArrival;
    [SerializeField] private AudioClip trainLeave;
    [SerializeField] private AudioClip trainPassBy;
    [SerializeField] private AudioClip backgroundTalking;
    [SerializeField] private AudioClip walletClick;
    [SerializeField] private AudioClip cardHover;
    [SerializeField] private AudioClip cafeBackgroundTalking;
    [SerializeField] private AudioClip click;
    [SerializeField] private AudioClip menuClick;
    [SerializeField] private AudioClip dialogueContinue;
    [SerializeField] private AudioClip responseClick;
    [SerializeField] private AudioClip phoneMessageReceive;
    [SerializeField] private AudioClip phoneMessageDelivered;
    [SerializeField] private AudioClip phoneTyping;
    [SerializeField] private AudioClip unoGrabCard;
    [SerializeField] private AudioClip unoLayDownCard;
    [SerializeField] private AudioClip battleshipGrabBoat;
    [SerializeField] private AudioClip battleshipSetBoat;
    [SerializeField] private AudioClip battleshipSetPin;
    [SerializeField] private AudioClip catPurr;
    [SerializeField] private AudioClip cafeBackgroundMusic;
    [SerializeField] private AudioClip itemsSpawn;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public static float PlaySound(Sounds soundToPlay, AudioSource source)
    {
        if (source.isPlaying)
            source.Stop();

        AudioClip clip = instance.GetClip(soundToPlay);
        source.clip = clip;
        source.Play();
        return clip.length;
    }

    public static float PlayOneShot(Sounds soundToPlay, AudioSource source)
    {
        AudioClip clip = instance.GetClip(soundToPlay);
        source.PlayOneShot(clip);
        return clip.length;
    }

    private AudioClip GetClip(Sounds sound)
    {
        switch (sound)
        {
            default:
            case Sounds.None:
                return null;
            case Sounds.TrainArrival:
                return trainArrival;
            case Sounds.TrainLeave:
                return trainLeave;
            case Sounds.TrainPassBy:
                return trainPassBy;
            case Sounds.BackgroundTalking:
                return backgroundTalking;
            case Sounds.WalletClick:
                return walletClick;
            case Sounds.CardHover:
                return cardHover;
            case Sounds.CafeBackgroundTalking:
                return cafeBackgroundTalking;
            case Sounds.Click:
                return click;
            case Sounds.MenuClick:
                return menuClick;
            case Sounds.DialogueContinue:
                return dialogueContinue;
            case Sounds.ResponseClick:
                return responseClick;
            case Sounds.PhoneMessageReceive:
                return phoneMessageReceive;
            case Sounds.PhoneMessageDelivered:
                return phoneMessageDelivered;
            case Sounds.PhoneTyping:
                return phoneTyping;
            case Sounds.UnoGrabCard:
                return unoGrabCard;
            case Sounds.UnoLayDownCard:
                return unoLayDownCard;
            case Sounds.BattleshipGrabBoat:
                return battleshipGrabBoat;
            case Sounds.BattleshipSetBoat:
                return battleshipSetBoat;
            case Sounds.BattleshipSetPin:
                return battleshipSetPin;
            case Sounds.CatPurr:
                return catPurr;
            case Sounds.CafeBackgroundMusic:
                return cafeBackgroundMusic;
            case Sounds.ItemsSpawn:
                return itemsSpawn;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
