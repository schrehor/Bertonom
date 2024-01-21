using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] FacingDirection defaultDirection = FacingDirection.Down;

    // Parameters
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }
    public bool IsJumping { get; set; }

    // States
    SpriteAnimator walkDownAnim;
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkRightAnim;
    SpriteAnimator walkLeftAnim;

    SpriteAnimator currentAnim;
    bool wasPrevMoving;

    // References
    SpriteRenderer spriteRenderer;

    public FacingDirection DefaultDirection { get => defaultDirection; }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);

        SetFacingDirection(defaultDirection);
        currentAnim = walkDownAnim;
    }

    private void Update()
    {
        var prevAnim = currentAnim;

        if (MoveX == 1)
        {
            currentAnim = walkRightAnim;
        }
        else if (MoveX == -1)
        {
            currentAnim = walkLeftAnim;
        }
        else if (MoveY == 1)
        {
            currentAnim = walkUpAnim;
        }
        else if (MoveY == -1)
        {
            currentAnim = walkDownAnim;
        }

        if (currentAnim != prevAnim || IsMoving != wasPrevMoving )
        {
            currentAnim.Start();
        }

        if (IsJumping)
        {
			spriteRenderer.sprite = currentAnim.Frames[currentAnim.Frames.Count - 1];
		}
        else if (IsMoving)
        {
            currentAnim.HandleUpdate();
        }
        else
        {
            spriteRenderer.sprite = currentAnim.Frames[0];
        }

        wasPrevMoving = IsMoving;
    }

    public void SetFacingDirection(FacingDirection direction)
    {
        switch (direction)
        {
            case FacingDirection.Up:
                MoveY = -1;
                break;
            case FacingDirection.Down:
                MoveY -= 1;
                break;
            case FacingDirection.Left:
                MoveX = -1;
                break;
            case FacingDirection.Right:
                MoveX = 1;
                break;
            default:
                break;
        }
    }
}

public enum FacingDirection { Up, Down, Left, Right }
