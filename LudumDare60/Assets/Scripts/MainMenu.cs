using UnityEngine;
using UnityEngine.UIElements;

namespace LD60
{
    /// <summary>
    /// Main menu painted on the wall: the UI Toolkit panel renders into the target texture of its PanelSettings, which the
    /// WallTexture material of this quad (1 x 1 unit, scaled by the transform) draws in the paint layer.
    /// The pointer position is projected on the quad plane to reach the panel. Keyboard and gamepad move the focus.
    /// The camera starts centered on the menu and the player waits for Begin (see PlayerDeath.WaitForBegin), while the
    /// music plays the menu loop (see MusicPlayer.WaitForBegin).
    /// Settings replaces the buttons with the volume sliders (see AudioPreferences); Back or the cancel input returns.
    /// Once Begin is pressed, the menu stays painted on the wall but can't be used anymore,
    /// until the camera comes back to it at the end of the game (see PlayerDeath.ReturnedToMenu).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenu : MonoBehaviour
    {
        const string HiddenClass = "menu-page--hidden";

        [SerializeField] PlayerDeath player;
        [SerializeField] CameraScroller scroller;
        [SerializeField] MusicPlayer music;

        UIDocument document;
        VisualElement root;
        VisualElement mainPage;
        VisualElement settingsPage;
        Button beginButton;
        Button settingsButton;
        Button quitButton;
        Button backButton;
        Slider soundSlider;
        Slider musicSlider;

        void Awake()
        {
            document = GetComponent<UIDocument>();
            if (player != null)
            {
                player.WaitForBegin = true;
            }
            if (music != null)
            {
                music.WaitForBegin = true;
            }
        }

        void OnEnable()
        {
            root = document.rootVisualElement;
            mainPage = root.Q("main-page");
            settingsPage = root.Q("settings-page");
            beginButton = root.Q<Button>("begin-button");
            settingsButton = root.Q<Button>("settings-button");
            quitButton = root.Q<Button>("quit-button");
            backButton = root.Q<Button>("back-button");
            soundSlider = root.Q<Slider>("sound-slider");
            musicSlider = root.Q<Slider>("music-slider");

            beginButton.clicked += Begin;
            settingsButton.clicked += OpenSettings;
            quitButton.clicked += Quit;
            backButton.clicked += CloseSettings;
            soundSlider.SetValueWithoutNotify(AudioPreferences.SoundVolume);
            musicSlider.SetValueWithoutNotify(AudioPreferences.MusicVolume);
            soundSlider.RegisterValueChangedCallback(evt => AudioPreferences.SoundVolume = evt.newValue);
            musicSlider.RegisterValueChangedCallback(evt => AudioPreferences.MusicVolume = evt.newValue);
            root.RegisterCallback<NavigationCancelEvent>(_ => CloseSettings());
            // Hovering an entry selects it, so that the mouse and the keyboard or gamepad share a single highlight.
            root.Query(className: "menu-entry").ForEach(entry => entry.RegisterCallback<PointerEnterEvent>(_ => entry.Focus()));

            // Application.Quit does nothing on the web.
            quitButton.style.display = Application.platform == RuntimePlatform.WebGLPlayer ? DisplayStyle.None : DisplayStyle.Flex;

            document.panelSettings.SetScreenToPanelSpaceFunction(ScreenToPanel);
            if (player != null)
            {
                player.ReturnedToMenu += Reopen;
            }
        }

        void OnDisable()
        {
            if (player != null)
            {
                player.ReturnedToMenu -= Reopen;
            }
        }

        void Start()
        {
            if (scroller != null)
            {
                scroller.MoveTo(scroller.CameraXFor(transform.position.x, 0.5f));
            }
            ShowPage(mainPage, settingsPage, beginButton);
        }

        void Begin()
        {
            root.focusController.focusedElement?.Blur();
            root.SetEnabled(false);
            if (player != null)
            {
                player.Begin();
            }
            if (music != null)
            {
                music.Begin();
            }
        }

        void Reopen()
        {
            root.SetEnabled(true);
            ShowPage(mainPage, settingsPage, beginButton);
        }

        void CloseSettings()
        {
            if (settingsPage.ClassListContains(HiddenClass))
            {
                return;
            }
            AudioPreferences.Save();
            ShowPage(mainPage, settingsPage, settingsButton);
        }

        void OpenSettings() => ShowPage(settingsPage, mainPage, soundSlider);

        void ShowPage(VisualElement shown, VisualElement hidden, Focusable focus)
        {
            shown.RemoveFromClassList(HiddenClass);
            hidden.AddToClassList(HiddenClass);
            focus.Focus();
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>Projects a screen position, measured from the top of the screen as UI Toolkit does, on the quad then in the panel texture.</summary>
        Vector2 ScreenToPanel(Vector2 screenPosition)
        {
            Vector2 invalid = new(float.NaN, float.NaN);
            RenderTexture target = document.panelSettings.targetTexture;
            if (scroller == null || target == null)
            {
                return invalid;
            }

            screenPosition.y = Screen.height - screenPosition.y;
            Ray ray = scroller.Camera.ScreenPointToRay(screenPosition);
            if (!new Plane(transform.forward, transform.position).Raycast(ray, out float distance))
            {
                return invalid;
            }

            Vector3 local = transform.InverseTransformPoint(ray.GetPoint(distance));
            if (Mathf.Abs(local.x) > 0.5f || Mathf.Abs(local.y) > 0.5f)
            {
                return invalid;
            }
            return new Vector2((local.x + 0.5f) * target.width, (0.5f - local.y) * target.height);
        }
    }
}
