namespace HKMP_HealthDisplay;

public class HealthBar : Container
    {
        // why do it like this? simply put, it reminds you to do null checking and makes stuff less likely to break in edge cases, e.g. accidentally destroying or replacing
        // the child in the lifecycle of this element
        private StackLayout? MaskDisplay => Child as StackLayout;

        public HealthBar(LayoutRoot onLayout, GameObject target, string name = "New Health display") : base(onLayout, name)
        {
            // this could be a DynamicUniformGrid with 9 children before rollover, if you wanted to make masks break onto a new line after 9 masks
            DynamicUniformGrid maskDisplay = new(onLayout, "HealthDisplay_" + target.name)
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                ChildrenBeforeRollover = 6,
                HorizontalSpacing = 2,
                Padding = new Padding(0, 2),
            };

            GameObjectFollowingLayout.ObjectToFollow.Set(this, target);

            // override the default alignments to new defaults
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.VerticalAlignment = VerticalAlignment.Bottom;

            // set the child in the layout system
            this.Child = maskDisplay;
        }

        // or handle adding and removing children in some more sophisticated way e.g. with a property
        public void SetMasks(int newMasks)
        {
            if (MaskDisplay != null)
            {
                int currentMasks = MaskDisplay.Children.Count;
                if (newMasks > currentMasks)
                {
                    for (int i = 0; i < newMasks - currentMasks; i++)
                    {
                        AddMask();
                    }
                }
                else if (newMasks < currentMasks)
                {
                    for (int i = 0; i < currentMasks - newMasks; i++)
                    {
                        RemoveMask();
                    }
                }
            }
        }
        private void AddMask()
        {
            if (MaskDisplay != null) 
            {
                MaskDisplay.Children.Add(new Image(this.LayoutRoot, AssetLoader.Mask));
            }
        }

        private void RemoveMask()
        {
            MaskDisplay?.Children.RemoveAt(0);
        }

        protected override Vector2 MeasureOverride()
        {
            // I am exactly as big as my child is (i.e. no additional internal spacing or whatever)
            return Child?.EffectiveSize ?? Vector2.zero;
        }

        protected override void ArrangeOverride(Vector2 alignedTopLeftCorner)
        {
            // arrange my child with its desired size in the position allocated to me
            Child?.Arrange(new Rect(alignedTopLeftCorner, Child.EffectiveSize));
        }

        protected override void DestroyOverride()
        {
            Child?.Destroy();
            Child = null;
        }
    }