using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace BlackBaron
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class PhysicsEngine : Microsoft.Xna.Framework.GameComponent
    {
        Game1 game;
        List<IPhysicalObject> PhysicalObjects = new List<IPhysicalObject>();
        Random randomizer;

        // Constants for controling horizontal movement
        //float MoveAcceleration = 15000.0f;
        //float MaxMoveSpeed = 1200.0f;
        //float GroundDragFactor = 0.38f;
        //float AirDragFactor = 0.50f;

        // Constants for controlling vertical movement
        //float MaxJumpTime = 0.35f;
        //float JumpLaunchVelocity = -4000.0f;
        private const float GravityAcceleration = 1500.0f;      // gravity
        private const float MaxFallSpeed = 1800.0f;              // this should be turned into the Intensity property
        //float JumpControlPower = 0.14f;

        //bool isOnGround = false;
        //bool isElemental = true;
        bool isSuspended = false;

        public void Suspend()
        {
            isSuspended = true;
        }

        public void Resume()
        {
            isSuspended = false;
        }

        public PhysicsEngine(Game1 game)
            : base(game)
        {
            this.game = game;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            PhysicalObjects = new List<IPhysicalObject>();
            randomizer = new Random();
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (isSuspended)
                return;
            double now = gameTime.TotalGameTime.TotalMilliseconds;
            double interval = gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = 0; i < PhysicalObjects.Count; i++)
            {

                float timeAlive = (float)(now - PhysicalObjects[i].BirthTime);
                if (timeAlive > PhysicalObjects[i].Life && PhysicalObjects[i].isElemental)
                {
                    PhysicalObjects.RemoveAt(i);
                    //break;
                }
                else
                {
                    float relAge = timeAlive / PhysicalObjects[i].Life;
                    // update position
                    PhysicalObjects[i].Position = PhysicalObjects[i].Acceleration + PhysicalObjects[i].Direction + PhysicalObjects[i].Position;
                    // rotate given sprites
                    PhysicalObjects[i].Rotation += PhysicalObjects[i].RotationFactor;
                    if (PhysicalObjects[i].Rotation >= Math.PI * 2)
                        PhysicalObjects[i].Rotation = 0.0f;

                    // apply gravity
                    PhysicalObjects[i].Position = new Vector2(PhysicalObjects[i].Position.X, MathHelper.Clamp((float)(PhysicalObjects[i].Position.Y + (PhysicalObjects[i].Weight * GravityAcceleration) * (timeAlive / 1000)* interval), -MaxFallSpeed, MaxFallSpeed));
                    // Base velocity is a combination of horizontal movement control and
                    // acceleration downward due to gravity.


                    ////////////////////////////////////////////////////////////////////

                    if (!PhysicalObjects[i].isElemental)
                    {
                        // Apply pseudo-drag horizontally.
                        //if (isOnGround)
                            //velocity.X *= GroundDragFactor;
                        //else
                            //velocity.X *= AirDragFactor;

                        // Prevent the player from running faster than his top speed.            
                        //velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);
                    }
                }
            }
            base.Update(gameTime);
        }

        public void RegisterObject(IPhysicalObject physObject)
        {
            PhysicalObjects.Add(physObject);                    // you may have a problem with deep copy v. shallow copy with this; you want a shallow copy so the physics engine will manipulate the original class
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        /*private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }*/
    }
}