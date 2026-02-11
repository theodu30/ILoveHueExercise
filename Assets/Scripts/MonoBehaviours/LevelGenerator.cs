using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class LevelGenerator : MonoBehaviour
{
    public enum LockPattern
    {
        None,
        Borders,
        Checkerboard,
        InverseCheckerboard,
        Columns,
        Rows,
        InverseColumns,
        InverseRows,
        Corners
    }

    private UIDocument levelDocument;
    private VisualElement root;

    public LockPattern Pattern = LockPattern.None;
    public int HueRows = 11;
    public int HueColumns = 3;
    private bool grabbed = false;
    private VisualElement grabbedElement = null;
    private Vector2 previousPos = Vector2.zero;

    public Color UpperLeftColor = Color.yellow;
    public Color UpperRightColor = Color.yellow;
    public Color LowerLeftColor = Color.red;
    public Color LowerRightColor = Color.red;

    private bool levelComplete = false;

    private void Awake()
    {
        levelDocument = GetComponent<UIDocument>();
        root = levelDocument.rootVisualElement;

        for (int row = 0; row < HueRows; row++)
        {
            for (int col = 0; col < HueColumns; col++)
            {
                VisualElement hueSlot = CreateHueSlot(col, row);

                hueSlot.style.backgroundColor = GetInterpolatedColor(
                    col / (float)(HueColumns),
                    row / (float)(HueRows)
                );

                hueSlot.name = $"HueSlot_{row}_{col}";

                if (IsPositionLocked(col, row))
                {
                    LockElement(ref hueSlot);
                }

                root.Add(hueSlot);
            }
        }

        do
        {
            ShuffleLevel();
        } while (IsLevelComplete());
    }

    private void Update()
    {
        if (levelComplete) return;

        if (IsLevelComplete())
        {
            levelComplete = true;
            GameEvents.GameWin();
            return;
        }

        if (levelDocument == null) return;

        Vector2 touchPos = -Vector2.one; // Out of bounds
        bool isTouch = false;

        if (!grabbed)
        {
            if (IsTouchStarted())
            {
                touchPos = GetTouchPosition();
                isTouch = true;
            }
        }
        else
        {
            if (IsTouchEnded() && grabbedElement != null)
            {
                grabbed = false;

                Vector2 currentPos = GetTouchPosition();
                currentPos.y = Screen.height - currentPos.y;

                Vector2 gridPos = GetPositionOntoGrid(currentPos);
                SetGrabbedElementPosition(gridPos);
                grabbedElement.SendToBack();

                VisualElement occupyingElement = root.panel.Pick(gridPos);
                if (occupyingElement != null && occupyingElement is not Button && occupyingElement != grabbedElement)
                {
                    if (occupyingElement.name.EndsWith("_Locked") && occupyingElement.name.StartsWith("HueSlot"))
                    {
                        SetElementPosition(grabbedElement, previousPos);
                    }
                    else
                    {
                        Vector2 occupyPos = new Vector2(
                            occupyingElement.resolvedStyle.left + occupyingElement.resolvedStyle.width / 2f,
                            occupyingElement.resolvedStyle.top + occupyingElement.resolvedStyle.height / 2f
                        );
                        SetElementPosition(occupyingElement, previousPos);
                        SetElementPosition(grabbedElement, occupyPos);
                    }
                }

                grabbedElement = null;
            }
            else
            {
                Vector2 currentPos = GetTouchPosition();
                currentPos.y = Screen.height - currentPos.y;
                SetGrabbedElementPosition(currentPos);
            }
        }

        if (isTouch)
        {
            touchPos.y = Screen.height - touchPos.y;
            VisualElement pickedElement = root.panel.Pick(touchPos);
            if (pickedElement != null)
            {
                if (!grabbed && !pickedElement.name.EndsWith("_Locked") && pickedElement.name.StartsWith("HueSlot"))
                {
                    grabbed = true;
                    grabbedElement = pickedElement;
                    grabbedElement.BringToFront();
                    previousPos = new Vector2(
                        grabbedElement.resolvedStyle.left + grabbedElement.resolvedStyle.width / 2f,
                        grabbedElement.resolvedStyle.top + grabbedElement.resolvedStyle.height / 2f
                    );
                    SetGrabbedElementPosition(touchPos);
                }
            }
        }
    }

    private VisualElement CreateHueSlot(float posx, float posy)
    {
        VisualElement hueSlot = new VisualElement();

        hueSlot.style.position = Position.Absolute;
        hueSlot.style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50), 0);
        hueSlot.style.width = Length.Percent(100f / HueColumns);
        hueSlot.style.height = Length.Percent(100f / HueRows);
        hueSlot.style.left = Length.Percent(posx * (100f / HueColumns));
        hueSlot.style.top = Length.Percent(posy * (100f / HueRows));

        return hueSlot;
    }

    private Color GetInterpolatedColor(float xPercent, float yPercent)
    {
        Color topColor = Color.Lerp(UpperLeftColor, UpperRightColor, xPercent);
        Color bottomColor = Color.Lerp(LowerLeftColor, LowerRightColor, xPercent);
        return Color.Lerp(topColor, bottomColor, yPercent);
    }

    private void SetGrabbedElementPosition(Vector2 position)
    {
        if (grabbedElement != null)
        {
            grabbedElement.style.left = Length.Pixels(position.x - grabbedElement.resolvedStyle.width / 2f);
            grabbedElement.style.top = Length.Pixels(position.y - grabbedElement.resolvedStyle.height / 2f);
        }
    }

    private void SetElementPosition(in VisualElement element, Vector2 position)
    {
        if (element != null)
        {
            element.style.left = Length.Pixels(position.x - element.resolvedStyle.width / 2f);
            element.style.top = Length.Pixels(position.y - element.resolvedStyle.height / 2f);
        }
    }

    private Vector2 GetPositionOntoGrid(Vector2 position)
    {
        float slotWidth = Screen.width / HueColumns;
        float slotHeight = Screen.height / HueRows;
        int col = Mathf.Clamp(Mathf.FloorToInt(position.x / slotWidth), 0, HueColumns - 1);
        int row = Mathf.Clamp(Mathf.FloorToInt(position.y / slotHeight), 0, HueRows - 1);
        float snappedX = col * slotWidth + slotWidth / 2f;
        float snappedY = row * slotHeight + slotHeight / 2f;
        return new Vector2(snappedX, snappedY);
    }

    private bool IsPositionLocked(int col, int row)
    {
        switch (Pattern)
        {
            case LockPattern.None:
                return false;
            case LockPattern.Borders:
                return row == 0 || row == HueRows - 1 || col == 0 || col == HueColumns - 1;
            case LockPattern.Checkerboard:
                return (row + col) % 2 == 0;
            case LockPattern.InverseCheckerboard:
                return (row + col) % 2 != 0;
            case LockPattern.Columns:
                return col % 2 == 0;
            case LockPattern.Rows:
                return row % 2 == 0;
            case LockPattern.InverseColumns:
                return col % 2 != 0;
            case LockPattern.InverseRows:
                return row % 2 != 0;
            case LockPattern.Corners:
                return (row == 0 || row == HueRows - 1) && (col == 0 || col == HueColumns - 1);
            default:
                return false;
        }
    }

    private void LockElement(ref VisualElement element)
    {
        element.name += "_Locked";
        VisualElement lockOverlay = new VisualElement();
        lockOverlay.style.position = Position.Absolute;
        // Centered black point
        lockOverlay.style.width = Length.Percent(20);
        lockOverlay.style.height = Length.Percent(20);
        lockOverlay.style.left = Length.Percent(40);
        lockOverlay.style.top = Length.Percent(40);
        lockOverlay.style.backgroundColor = Color.black;
        lockOverlay.name = "_Locked";
        element.Add(lockOverlay);
    }

    private bool IsElementAtRightPosition(VisualElement element)
    {
        if (element == null) return false;
        // Extract row and column from the name
        // Name format: "HueSlot_{row}_{col}"
        string[] parts = element.name.Split('_');
        if (parts.Length < 3) return false;
        int row = int.Parse(parts[1]);
        int col = int.Parse(parts[2]);
        float slotWidth = Screen.width / HueColumns;
        float slotHeight = Screen.height / HueRows;
        float expectedX = col * slotWidth + slotWidth / 2f;
        float expectedY = row * slotHeight + slotHeight / 2f;
        float actualX = element.resolvedStyle.left + element.resolvedStyle.width / 2f;
        float actualY = element.resolvedStyle.top + element.resolvedStyle.height / 2f;
        // 5% tolerance
        float toleranceX = slotWidth * 0.05f;
        float toleranceY = slotHeight * 0.05f;
        return Mathf.Abs(expectedX - actualX) <= toleranceX && Mathf.Abs(expectedY - actualY) <= toleranceY;
    }

    private bool IsLevelComplete()
    {
        foreach (VisualElement element in root.Children())
        {
            if (!IsElementAtRightPosition(element))
            {
                return false;
            }
        }
        return true;
    }

    private void ShuffleLevel()
    {
        List<VisualElement> elements = new List<VisualElement>();
        List<Vector2> positions = new List<Vector2>();
        float slotWidth = Screen.width / HueColumns;
        float slotHeight = Screen.height / HueRows;

        foreach (VisualElement element in root.Children())
        {
            if (!element.name.EndsWith("_Locked"))
            {
                elements.Add(element);

                string[] parts = element.name.Split('_');
                int row = int.Parse(parts[1]);
                int col = int.Parse(parts[2]);
                Vector2 pos = new Vector2(
                    col * slotWidth + slotWidth / 2f,
                    row * slotHeight + slotHeight / 2f
                );

                positions.Add(pos);
            }
        }

        System.Random rand = new System.Random();
        positions = positions.OrderBy(x => rand.Next()).ToList();
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].style.left = Length.Pixels(positions[i].x - slotWidth / 2f);
            elements[i].style.top = Length.Pixels(positions[i].y - slotHeight / 2f);
        }
    }

    private bool IsTouchStarted()
    {
        if (Touchscreen.current != null)
        {
            return Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        }
        else if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }
        return false;
    }

    private bool IsTouchEnded()
    {
        if (Touchscreen.current != null)
        {
            return Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
        }
        else if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasReleasedThisFrame;
        }
        return false;
    }

    private Vector2 GetTouchPosition()
    {
        if (Touchscreen.current != null)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
        return Vector2.zero;
    }

    private bool IsTouchPressed()
    {
        if (Touchscreen.current != null)
        {
            return Touchscreen.current.primaryTouch.press.isPressed;
        }
        else if (Mouse.current != null)
        {
            return Mouse.current.leftButton.isPressed;
        }
        return false;
    }
}
