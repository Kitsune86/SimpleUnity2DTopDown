//Simple 2D shooter movement and custom controls (X-input compatble devices). This code will be licensed under the MIT License for use by anyone.
//This code does make specific reference to Systems designed outside of the scope of publication. Documentation will denote where this occurs.
//Code written by Rae Michelle Richards - 2018 - 2022
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Note use of UnityEngine.SceneManagement and Unity.Postprocessing here. We will be using these for events upon player death.
using UnityEngine.SceneManagement;
using UnityEngine.PostProcessing;

public class MovementXbox : MonoBehaviour
{
    //When polling presses for buttons we're going to use several variables across frames. Currentpress is the timestamp for the current frame.
    //nextpress is the float for the next allowable press to avoid accepting unwanted input. Nextpress is set upon a valid registered input.
    public float currentpress;
    public float nextpress;
    //Simple variables so we know the player's X and Y co-ords without having to pull from the gameobject every time. Rarely used.
    private float x;
    private float y;
    // "Shipbody" refers to the rigidbody attached to our player ship. Used for applying forces and an integral part of collision detection.
    private Rigidbody2D shipbody;
    //Angularspeed and forwaedvelocity allow us to access the player ship object's speed and velocity respectively. 
    [SerializeField] private float angularspeed;
    [SerializeField] private float forwardvelocity;
    //Engine Game Objects are the 'physical' position of the engines on the player ship. Attached to the engines are animators for engine exhuast sprites.
    [SerializeField] private GameObject Engine1;
    [SerializeField] private GameObject Engine2;
    [SerializeField] private Animator Engine1Anim;
    [SerializeField] private Animator Engine2Anim;
    //EnergyProjectile is the internal name for the simple sprite based projectiles the player ship shoots.
    [SerializeField] private GameObject EnergyProjectile;
    //Projectilebody is the rigidbody that we will have to manipulate to fire projectiles.
    [SerializeField] private Rigidbody2D ProjectileBody;
    //Since this code was written for a simple screen space shoot there are several positional markers with collision in each corner.
    //Hitting one of these 8 positions will determine where we will warp the player ship. A form of "screen scrolling".
    [SerializeField] private GameObject DownSpawn;
    [SerializeField] private GameObject UpSpawn;
    [SerializeField] private GameObject RightSpawn;
    [SerializeField] private GameObject LeftSpawn;
    [SerializeField] private GameObject UpLeftSpawn;
    [SerializeField] private GameObject UpRightSpawn;
    [SerializeField] private GameObject DownLeftSpawn;
    [SerializeField] private GameObject DownRightSpawn;
    //Upon contact with the above Spawns we need to set a flag to activate movement thus the below:
    [SerializeField] private bool LeftTouch = false;
    [SerializeField] private bool RightTouch = false;
    [SerializeField] private bool DownTouch = false;
    [SerializeField] private bool UpTouch = false;
    //We need to know how many times the player ship has been hit. INT for that.
    [SerializeField] private int NumberofHits = 0;
    //The below gameobjects are as follows:
    //Explosion - an explosion sprite created upon death.
    //GameOverMessage - a sprite of a message telling the playeer they've failed (later replaced with a full Unity Scene)
    [SerializeField] private GameObject Explosion;
    [SerializeField] private GameObject GameOverMessage;
    //Eventmanager is a componet tied to an invisible GameObject that controls things like amount of time the level has been active.
    //when to spawn waves of enemies and making sure that the player is not out of bounds.
    [SerializeField] private GameObject EventManager;
	[SerializeField] private Camera GameCamera;
    //Accessing a pre-placed Unity Post Processor so we can do cool things with the screen when the player takes a hit.
    //We also have to access the Profile in addition to the Behavior or none of this will function. (Introduced in Unity 5)
	[SerializeField] private PostProcessingBehaviour PostProccessTTS;
	[SerializeField] private PostProcessingProfile PostProccessPRO;
    //Simple flag so we know if game is in Dev (God Mode)
    //Simple flags so we know the state of the game and can react - is the player dead? is the game paused?
    [SerializeField] private bool DevModeGodMode = false;
    [SerializeField] bool IsPlayerDead = false;
    [SerializeField] bool IsGamePaused = false;
    //Vent objects are used to play animations when the player turns. Sprite based plumes of gas.
    [SerializeField] GameObject VentLeft;
    [SerializeField] GameObject VentRight;
    //Initalize Audiosources for firing projectiles and the sounds of engines. As well as a Musicplayer.
    [SerializeField] AudioSource EngineSound;
    [SerializeField] AudioSource LazerSound;
	[SerializeField] private AudioSource MusicPlayer;
    [SerializeField] private bool HasPlayStopped = false;
    //We need to set the player's health to something so they don't die immediately. Health is also tied to array of sprites so it can be visually displayed.
    [SerializeField] private int HealthLevel = 3;
    [SerializeField] private Sprite[] HealthSprites;
    [SerializeField] private GameObject HealthMeter;
    //Left over debug variable to see if the joystick on an X-Input device has been pressed. Leaving in as to not break things.
    [SerializeField] private bool joystickpressed = false;

    // Use this for initialization
    void Start()
    {
        //Set the timescale to 1 at initalization. Don't want physics to go nyooom really fast.
        Time.timeScale = 1;
        nextpress = 0;
        //Define the Shipbody, Engine Animators and Post Processors once on start so we don't have to make redundant calls every time.
        shipbody = this.gameObject.GetComponent<Rigidbody2D>();
        Engine1Anim = Engine1.GetComponent<Animator>();
        Engine2Anim = Engine2.GetComponent<Animator>();
		PostProccessTTS = GameCamera.GetComponent<PostProcessingBehaviour>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //Upon touching any of the barriers setup around the arena set a boolean flag so that the game can react. Code could be refactored
        //here so that the events happen at the same time. Seperated out just for visual seperation during the debug proccess.
        //NOTE TO SELF: CONSIDER REFACTORING
        if (collision.gameObject.name == "UpBoundry")
        {
            UpTouch = true;
        }
        if (collision.gameObject.name == "DownBoundry")
        {
            DownTouch = true;
        }
        if (collision.gameObject.name == "RightBoundry")
        {
            RightTouch = true;
        }
        if (collision.gameObject.name == "LeftBoundry")
        {
            LeftTouch = true;
        }
        if (collision.gameObject.name == "DownLeftBoundry")
        {
            DownTouch = true;
            LeftTouch = true;
        }
        if (collision.gameObject.name == "DownRightBoundry")
        {
            DownTouch = true;
            RightTouch = true;
        }
        if (collision.gameObject.name == "UpLeftBoundry")
        {
            UpTouch = true;
            LeftTouch = true;
        }
        if (collision.gameObject.name == "UpRightBoundry")
        {
            UpTouch = true;
            RightTouch = true;
        }
        //Conditional statements for moving the ship when it hits various boundary states. Attempts to also account for when touching the edge of
        //two different boundries by including double true evaluations. Which would result in ship spawning in a lower corner.
        //Example: Hitting right boundry and top boundry intersection (top corner) would send ship to lower left spawner.
        //also note the Adjust Rotation function call after changing the ship location. This is done so the ship doesn't spawn backwards in
        //new location.
        if ((DownTouch == true) && (RightTouch == true))
        {
            var currentx = UpLeftSpawn.transform.position.x;
            var newy = UpLeftSpawn.transform.position.y;
            var currentrotation = UpLeftSpawn.transform.rotation;
            this.transform.position = new Vector3(currentx, newy, -12);
            StartCoroutine(AdjustRotation());
            RightTouch = false;
            DownTouch = false;
        }
        //Same as above comments regarding moving the ship
        if ((DownTouch == true) && (LeftTouch == true))
        {
            var currentx = UpRightSpawn.transform.position.x;
            var newy = UpRightSpawn.transform.position.y;
            var currentrotation = UpRightSpawn.transform.rotation;
            this.transform.position = new Vector3(currentx, newy, -12);
            StartCoroutine(AdjustRotation());
            LeftTouch = false;
            DownTouch = false;
        }
        //Same as above comments regarding moving the ship
        if ((UpTouch == true) && (LeftTouch == true))
        {
            var currentx = DownRightSpawn.transform.position.x;
            var newy = DownRightSpawn.transform.position.y;
            var currentrotation = DownRightSpawn.transform.rotation;
            this.transform.position = new Vector3(currentx, newy, -12);
            StartCoroutine(AdjustRotation());
            LeftTouch = false;
            UpTouch = false;
        }
        //Same as above comments regarding moving the ship
        if ((UpTouch == true) && (RightTouch == true))
        {
            var currentx = DownLeftSpawn.transform.position.x;
            var newy = DownLeftSpawn.transform.position.y;
            var currentrotation = DownLeftSpawn.transform.rotation;
            this.transform.position = new Vector3(currentx, newy, -12);
            StartCoroutine(AdjustRotation());
            RightTouch = false;
            UpTouch = false;
        }
        //Same as above comments regarding moving the ship
        if (((RightTouch == true) && (UpTouch == false)) || ((RightTouch == true) && (DownTouch == false)))
        {
            var currentx = LeftSpawn.transform.position.x;
            var newy = this.transform.position.y;
            var currentrotation = this.transform.rotation;
            this.transform.position = new Vector3(currentx, newy, -12);
            StartCoroutine(AdjustRotation());
            RightTouch = false;
        }
        if (((LeftTouch == true) && (UpTouch == false)) || ((LeftTouch == true) && (DownTouch == false)))
        {
            var currentx = RightSpawn.transform.position.x;
            var newy = this.transform.position.y;
            var currentrotation = this.transform.rotation;
            this.transform.position = new Vector3(currentx, newy, -12);
            StartCoroutine(AdjustRotation());
            LeftTouch = false;
        }
        //Same as above comments regarding moving the ship
        if (((UpTouch == true) && (RightTouch == false)) || ((UpTouch == true) && (LeftTouch == false)))
        {
            var currentx = this.transform.position.x;
            var newy = DownSpawn.transform.position.y;
            var currentrotation = this.transform.rotation;
            this.transform.position = new Vector3(currentx, newy, -12);
            StartCoroutine(AdjustRotation());
            UpTouch = false;
        }
        if (((DownTouch == true) && (RightTouch == false)) || ((DownTouch == true) && (LeftTouch == false)))
        {
            var currentx = this.transform.position.x;
            var newy = UpSpawn.transform.position.y;
            //var currentrotation = this.transform.rotation;
            this.transform.position = new Vector3(currentx, newy, -12);
            StartCoroutine(AdjustRotation());
            DownTouch = false;
        }
    // Update is called once per frame
    void Update()
    {
        //Once perframe check if the event manager has suspended play. This is important so that we can restart gameplay from begining in current
        //debug state. In later versions this will actually pause and unpause all actions by setting physics timescale to 0.
        HasPlayStopped = EventManager.GetComponent<EventManager>().StopCounting;
        if (HasPlayStopped == true)
        {
            if (Input.GetKey(KeyCode.Return))
            {
                //if event manager considers play stopped this is probably because something has gone terribly wrong. 
                //ie. Player out of bounds. Wave of enemies failing to spawn. Health values going out of bounds.
                //An error catch all
                var CurrentScene = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(0, LoadSceneMode.Single);
            }
        }
        //Simply updating the UI display to reflect plyer's current health.
        //NOTE TO SELF: Future optimization. These calls don't need to happen every frame. Move to every time player is hit.
        if (HealthLevel == 3)
        {
            HealthMeter.GetComponent<SpriteRenderer>().sprite = HealthSprites[0];
        }
        if (HealthLevel == 2)
        {
            HealthMeter.GetComponent<SpriteRenderer>().sprite = HealthSprites[1];
        }
        if (HealthLevel == 1)
        {
            HealthMeter.GetComponent<SpriteRenderer>().sprite = HealthSprites[2];
        }
        if (HealthLevel == 0)
        {
            HealthMeter.GetComponent<SpriteRenderer>().sprite = HealthSprites[3];
        }
        //If the player happens to die we need to do a number of things in sequence to maintain the gameplay loop.
        if (IsPlayerDead == true)
        {
            //check if God Mode (Dev) is enabled. If its enabled we are skipping all this stuff they can fly around forever.
            if (DevModeGodMode == false)
            {
                //Start co-rountine to reboot the game.
                //Tell the input manager to cease all physics calcs and time keeping or statekeeping.
                StartCoroutine(RestartGame());
                EventManager.GetComponent<EventManager>().StopCounting = true;
                //When game ends and message is displayed rebuild the index of available scenes since we didn't do that and transition to new scene
                if (Input.GetKey(KeyCode.JoystickButton7) == true)
                {
                    var CurrentScene = SceneManager.GetActiveScene().buildIndex;
                    SceneManager.LoadScene(CurrentScene, LoadSceneMode.Single);
                }
            }
        }
        //Assign variables created at program execution once per frame since we could be polling inputs multiple tmes per second.
        //Angular speed is the ships current Velocity represented numericly.
        //Forwardvelocity is the amount of force magnitude applied to the rigidbody. 
        angularspeed = shipbody.angularVelocity;
        forwardvelocity = shipbody.velocity.magnitude;
        x = this.transform.position.y;
        y = this.transform.position.y;
        //Set current allowed press to the current delta time. Optimization note: this line isn't using deltatime. Bad Rae D:
        currentpress = Time.time;
        if (Input.GetKeyDown(KeyCode.F1))
        {
            //if current press is greater than the timestamp for the next allowed press do various things.
            //Debug print statements about dev gode mode when testing controller input. Leaving in as a useful example to validate inputs.
            if ((currentpress > nextpress) && (DevModeGodMode == false))
            {
                DevModeGodMode = true;
                print("Debug: God Mode On");
                nextpress = currentpress + 0.07f;
            }
            if ((currentpress > nextpress) && (DevModeGodMode == true))
            {
                DevModeGodMode = false;
                print("Debug: God Mode Off");
                nextpress = currentpress + 0.07f;
            }
        }
        //Xbox Specific Axis - In input manager Axis 9 is our left joystick controlling left and right rotation. 
        //When fiting a projectile we want to make sure the ship is currently not in the process of being turned so we check to make sure the input
        //from that joysticks is 0 before firing.
        //a new projectile is created at this time and force is applied slightly faster than ship velocity so we never hit the ship with our own fire.
        //projectile is fired from an invisible object just above the nose of the ship.
        if ((Input.GetAxis("Axis 9") > 0f) || (Input.GetKey(KeyCode.JoystickButton0)))
        {
            if (joystickpressed == false)
            {
                if (currentpress > nextlaserpress)
                {
                    nextlaserpress = currentpress + 0.20f;
                    var NewProjectile = Instantiate(EnergyProjectile);
                    NewProjectile.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -11);
                    NewProjectile.transform.rotation = this.gameObject.transform.rotation;
                    NewProjectile.GetComponent<ProjectileDeath>().enabled = true;
                    ProjectileBody = NewProjectile.GetComponent<Rigidbody2D>();
                    ProjectileBody.AddForce((this.transform.right) * 0.060f);
                    if (!LazerSound.isPlaying)
                    {
                        LazerSound.Play();
                    }
                }
            }
        }
//This either button press (or Joystick press if using X-input device like an Xbox controller) moves the ship.
//When the expected input happens begin the coroutine to animate the engines. Add force to the ship as well.
//Also note that we will check if the audio sound player for the engines is playing. If not play the engine sound to the player.
        if ((Input.GetAxis("Axis 10") > 0f) || (Input.GetAxis("Axis 5") < 0f) || (Input.GetKey(KeyCode.JoystickButton1)))
        {
            joystickpressed = true;
            if (currentpress > nextpress)
            {
                nextpress = currentpress + 0.07f;
                shipbody.AddForce(transform.right * 0.95f, ForceMode2D.Impulse);
                StartCoroutine(EngineBurn());
                if (!EngineSound.isPlaying)
                {
                    EngineSound.Play();
                }
            }
        }
        else
        {
            joystickpressed = false;
        }
//This either button press (or Joystick press if using X-input device like an Xbox controller) turn the ship.
//When the expected input happens begin the coroutine to animate the side vents. Add force to the ship as well.
        if (Input.GetAxis("Axis 1") < 0f)
        {
            if (currentpress > nextventpress)
            {
                nextventpress = currentpress + 0.05f;
                StartCoroutine(VentLeftSide());
                if (forwardvelocity == 0)
                {
                    shipbody.AddTorque(-0.1f, ForceMode2D.Impulse);
                }
                else
                {
                    shipbody.AddTorque(-0.3f, ForceMode2D.Impulse);
                }
            }
        }
        if (Input.GetAxis("Axis 1") > 0f)
        {
            if (currentpress > nextventpress)
            {
                nextventpress = currentpress + 0.05f;
                StartCoroutine(VentRightSide());
                if (forwardvelocity == 0)
                {
                    shipbody.AddTorque(0.1f, ForceMode2D.Impulse);
                }
                else
                {
                    shipbody.AddTorque(0.3f, ForceMode2D.Impulse);
                }
            }

        }
        //Debug code to report to the eventmanager that we've decided to pause the game so it can take action.
        //Debug code left in. Currently functionality incomplete.
        if (Input.GetKeyDown(KeyCode.F2))
        {
            IsGamePaused = EventManager.GetComponent<EventManager>().PauseGame;
            if (IsGamePaused == true)
            {
                nextpress = currentpress + 0.07f;
                var TMPManager = EventManager.GetComponent<EventManager>();
                TMPManager.PauseTheGame(false);
            }
            if (IsGamePaused == false)
            {
                nextpress = currentpress + 0.07f;
                var TMPManager = EventManager.GetComponent<EventManager>();
                TMPManager.PauseTheGame(true);
            }
        }
    }
    //Ienumerator for restarting the game under various conditions. Set the timescale to 0 so physics stops and report to player the game is over.
    public IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(0.5f);
        Time.timeScale = 0;
        GameOverMessage.GetComponent<SpriteRenderer>().enabled = true;

    }
    //Ienumerator to adjust the player ships position when warped across the screen so it's facing the right direction. Note use of
    //wait until end of frame. We don't want to apply this adjustment mid movemement.
    private IEnumerator AdjustRotation()
    {
        var oldz = this.transform.position.z;
        yield return new WaitForEndOfFrame();
        Quaternion.Euler(new Vector3(this.transform.position.x, this.transform.position.y, oldz));

    }
    //Function for when the player dies. Several things occur at this time.
    //Player ships sprite renderer is turned off.
    //Immediately an explosion is spawned at their current location.
    //Report to the event manager that the player has died.
    //Wait for 0.5 seconds before we give them the "restart the level" or game over message so animation can finish.
    private IEnumerator TimeToDie()
    {
        this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        this.gameObject.GetComponent<PolygonCollider2D>().enabled = false;
        var Newx = this.transform.position.x;
        var Newy = this.transform.position.y;
        IsPlayerDead = true;
        var NewExplosion = Instantiate(Explosion, new Vector3(Newx, Newy, -12), Quaternion.identity);
        NewExplosion.transform.localScale = new Vector3(0.4f, 0.4f, 0);
        NewExplosion.GetComponent<ExplosionDeath>().enabled = true;
        yield return new WaitForSeconds(0.5f);
        //Destroy(this.gameObject);
    }
    private IEnumerator VentLeftSide()
    {
        var animplay = VentLeft.GetComponent<Animator>();
        animplay.SetBool("TimeToVent", true);
        yield return new WaitForSeconds(0.5f);
        animplay.SetBool("TimeToVent", false);
    }
    private IEnumerator VentRightSide()
    {
        var animplay = VentRight.GetComponent<Animator>();
        animplay.SetBool("TimeToVent", true);
        yield return new WaitForSeconds(0.5f);
        animplay.SetBool("TimeToVent", false);
    }
    private IEnumerator EngineBurn()
    {
        Engine1Anim.SetInteger("EngineBurn", 1);
        Engine2Anim.SetInteger("EngineBurn", 1);
        yield return new WaitForSeconds(0.01f);
        Engine1Anim.SetInteger("EngineBurn", 0);
        Engine2Anim.SetInteger("EngineBurn", 0);
    }
    //Several things happen when the player takes a hit. These are timed out in fractions of a second (0.01) and may not be aparent to the player.
    //Developer god mode usually reserved for testing is turned on for aproximately 0.56 of a single second. This stops them from taking 
    //multiple hits in a row while chromatic abberation effects are being applied through the post processor.
    //chromatic abberation effects are adjusted in intensity every .12th of a second while sound is muted. Giving the impression to the player
    //that the ships systems have gone offline. 
    //Player doesn't need to know they're invincible during this time :)
	private IEnumerator TakeAHit()
	{
		DevModeGodMode = true;
		var NewVolume = 0.07f;
		MusicPlayer.volume = NewVolume;
		PostProccessPRO = PostProccessTTS.profile;
		PostProccessPRO.chromaticAberration.enabled = true;
		PostProccessPRO.vignette.enabled = true;
		var NewChromaticValue = PostProccessPRO.chromaticAberration.settings;
		var NewVinValue = PostProccessPRO.vignette.settings;
		NewChromaticValue.intensity = 0.2f; 
		PostProccessPRO.chromaticAberration.settings = NewChromaticValue;
		NewVinValue.intensity = 0.2f;
		PostProccessPRO.vignette.settings = NewVinValue;
		yield return new WaitForSeconds (0.12f);
		NewChromaticValue.intensity = 0.3f; 
		PostProccessPRO.chromaticAberration.settings = NewChromaticValue;
		NewVinValue.intensity = 0.3f;
		PostProccessPRO.vignette.settings = NewVinValue;
		yield return new WaitForSeconds (0.12f);
		NewChromaticValue.intensity = 0.4f; 
		PostProccessPRO.chromaticAberration.settings = NewChromaticValue;
		NewVinValue.intensity = 0.4f;
		PostProccessPRO.vignette.settings = NewVinValue;
		yield return new WaitForSeconds (0.12f);
		NewChromaticValue.intensity = 0.3f; 
		PostProccessPRO.chromaticAberration.settings = NewChromaticValue;
		NewVinValue.intensity = 0.3f;
		PostProccessPRO.vignette.settings = NewVinValue;
		yield return new WaitForSeconds (0.12f);
		NewChromaticValue.intensity = 0.2f; 
		PostProccessPRO.chromaticAberration.settings = NewChromaticValue;
		NewVinValue.intensity = 0.2f;
		PostProccessPRO.vignette.settings = NewVinValue;
		yield return new WaitForSeconds (0.12f);
		NewChromaticValue.intensity = 0.0f; 
		NewVinValue.intensity = 0.0f;
		PostProccessPRO.vignette.settings = NewVinValue;
		PostProccessPRO.chromaticAberration.settings = NewChromaticValue;
		PostProccessPRO.chromaticAberration.enabled = false;
		PostProccessPRO.vignette.enabled = false;
		DevModeGodMode = false;
		NewVolume = 0.14f;
		yield return new WaitForSeconds (0.2f);
		MusicPlayer.volume = NewVolume;
		}
}