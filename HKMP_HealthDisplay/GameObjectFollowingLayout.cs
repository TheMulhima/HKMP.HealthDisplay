namespace HKMP_HealthDisplay;

    public class GameObjectFollowingLayout : Layout
    {
        // attach a property to children so that it is possible to look up which object each child should follow.
        // should this property ever be changed on an element it should rearrange that element's parent (i.e. this layout)
        public static AttachedProperty<GameObject?> ObjectToFollow = new(null, ChangeAction.ParentArrange);

        public GameObjectFollowingLayout(LayoutRoot onLayout, string name = "New GameObjectFollowingLayout") : base(onLayout, name) { }

        protected override Vector2 MeasureOverride()
        {
            // take up the whole screen. This is not always super idiomatic but it is a good/easy way for a layout to indicate it is not a good candidate to
            // be a child of some other element, and allows it free reign of the screen space to arrange its children.
            // usually, you'd be doing some math regarding the results of the measurements of all the children to determine how big the panel should be overall
            foreach (ArrangableElement child in Children)
            {
                child.Measure();
            }
            return UI.Screen.size;
        }

        protected override void ArrangeOverride(Vector2 alignedTopLeftCorner)
        {
            foreach (ArrangableElement child in Children)
            {
                GameObject? objToFollow = ObjectToFollow.Get(child);
                if (objToFollow != null)
                {
                    // project the position of the game object into screen space (1920x1080, MagicUI handles additional scaling if needed)
                    Vector2 childAnchor = objToFollow.transform.position; // todo: actual logic here
                    // offset the position as needed to respect child alignment (i.e. the child will now be aligned to the discovered anchor point)
                    childAnchor.x -= child.HorizontalAlignment switch
                    {
                        HorizontalAlignment.Center => child.EffectiveSize.x / 2,
                        HorizontalAlignment.Right => child.EffectiveSize.x,
                        _ => 0
                    };
                    childAnchor.y -= child.VerticalAlignment switch
                    {
                        VerticalAlignment.Center => child.EffectiveSize.y / 2,
                        VerticalAlignment.Bottom => child.EffectiveSize.y,
                        _ => 0
                    };
                    child.Arrange(new Rect(childAnchor, child.EffectiveSize));
                }
            }
        }

        protected override void DestroyOverride()
        {
            Children.Clear();
        }
    }