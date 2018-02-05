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
    public class ParticleEngine : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game1 game;
        
        SpriteBatch spriteBatch;
        Random randomizer = new Random();
        public List<ParticleEffect> effectList = new List<ParticleEffect>();
        public delegate void CustomUpdate(ParticleEffect pEffect, GameTime gameTime);

        /// <summary>
        /// Initialize a particle effect. This overload presumes an emitter that's not a point and has one texture
        /// </summary>
        /// <param name="Texture">The texture to be used for the particle.</param>
        /// <param name="emitter">The source rectangle that provides an area where random particles will emit from.</param>
        /// <param name="newUpdate">The CustomUpdate delegate that is passed and run during each update if you want to supplement the physics engine. Nullable. Requires a function that has a ParticleEngine.ParticleEffect and a GameTime as parameters, returning void.</param>
        /// <param name="modColor">The color to shade the texture in.</param>
        /// <param name="rotation">This value indicates how quickly the given texture will rotate (0 for none). Between 0 and 1. The actual rotation is randomized with the value entered here as the maximum possible random value.</param>
        /// <param name="intensity">The speed at which the particles will move.</param>
        /// <param name="life">The total lifespan of a given particle. When a particle reaches its lifespan it's removed from the effect. Measured in milliseconds.</param>
        /// <param name="weight">The physical weight of the object. Gravity will affect the object based on the weight. A value of -1 will produce inverse gravity.</param>
        /// <param name="emissionDirection">The clockwise limit of your emission.</param>
        /// <param name="acceleration">A vector which can alter the direction of the particle depending on its age. Because the physics engine simply adds the calculated direction to the current position + acceleration, acceleration of any vector will anchor particles to more than one direction (good especially for providing a unit vector to encourage downward falling random particles, fore example)</param>
        /// <param name="adding">Is this effect something that continuously runs until asked to stop? If so, use StopEffect() to halt particle generation.</param>
        /// <param name="randomScaling">Determines if the given effect should scale each particle randomly with the Scaling value as its maximum scale (ideal for snowflakes?)</param>
        /// <param name="numberOfParticles">The number of particles to generate with each update if adding is true. Otherwise, the total number of particles to generate.</param>
        /// <param name="range">The range of direction, in degrees.</param>
        /// <param name="size">this is the size of the effect's motion in a vector (but construed as a float for a vector's distance). Values grater than 1 will multiply the natural size of the effect. Values below 1 will reduce the natural size of the effect. You'll usually want to go a few orders of magnitude higher.</param>
        /// <param name="scaling">The scale to draw the image.</param>
        /// <param name="gameTime">Current gametime to update age.</param>
        public void AddEffect(Texture2D Texture, Rectangle emitter, CustomUpdate newUpdate, Color modColor, float rotation, float intensity, float life, float weight, int emissionDirection, Vector2 acceleration, bool adding, bool randomScaling, bool isPhysics, int numberOfParticles, int range, float size, float scaling, GameTime gameTime)
        {
            ParticleEffect effect = new ParticleEffect(game, Texture, emitter, newUpdate, modColor, rotation, intensity, life, weight, emissionDirection, acceleration, adding, randomScaling, isPhysics, numberOfParticles, range, size, scaling, gameTime);
            effectList.Add(effect);
        }

        /// <summary>
        /// Initialize a particle effect. This overload presumes an emitter that's not a point and has one texture
        /// </summary>
        /// <param name="Texture">The texture to be used for the particle.</param>
        /// <param name="emitter">The source point from which all particles will come from.</param>
        /// <param name="newUpdate">The CustomUpdate delegate that is passed and run during each update if you want to supplement the physics engine. Nullable. Requires a function that has a ParticleEngine.ParticleEffect and a GameTime as parameters, returning void.</param>
        /// <param name="modColor">The color to shade the texture in.</param>
        /// <param name="rotation">This value indicates how quickly the given texture will rotate (0 for none). Between 0 and 1. The actual rotation is randomized with the value entered here as the maximum possible random value.</param>
        /// <param name="intensity">The speed at which the particles will move.</param>
        /// <param name="life">The total lifespan of a given particle. When a particle reaches its lifespan it's removed from the effect. Measured in milliseconds.</param>
        /// <param name="weight">The physical weight of the object. Gravity will affect the object based on the weight. A value of -1 will produce inverse gravity.</param>
        /// <param name="emissionDirection">The clockwise limit of your emission.</param>
        /// <param name="acceleration">A vector which can alter the direction of the particle depending on its age. Because the physics engine simply adds the calculated direction to the current position + acceleration, acceleration of any vector will anchor particles to more than one direction (good especially for providing a unit vector to encourage downward falling random particles, fore example)</param>
        /// <param name="adding">Is this effect something that continuously runs until asked to stop? If so, use StopEffect() to halt particle generation.</param>
        /// <param name="randomScaling">Determines if the given effect should scale each particle randomly with the Scaling value as its maximum scale (ideal for snowflakes?)</param>
        /// <param name="numberOfParticles">The number of particles to generate with each update if adding is true. Otherwise, the total number of particles to generate.</param>
        /// <param name="range">The range of direction, in degrees.</param>
        /// <param name="size">this is the size of the effect's motion in a vector (but construed as a float for a vector's distance). Values grater than 1 will multiply the natural size of the effect. Values below 1 will reduce the natural size of the effect. You'll usually want to go a few orders of magnitude higher.</param>
        /// <param name="scaling">The scale to draw the image.</param>
        /// <param name="gameTime">Current gametime to update age.</param>
        public void AddEffect(Texture2D Texture, Vector2 emitter, CustomUpdate newUpdate, Color modColor, float rotation, float intensity, float life, float weight, int emissionDirection, Vector2 acceleration, bool adding, bool randomScaling, bool isPhysics, int numberOfParticles, int range, float size, float scaling, GameTime gameTime)
        {
            ParticleEffect effect = new ParticleEffect(game, Texture, emitter, newUpdate, modColor, rotation, intensity, life, weight, emissionDirection, acceleration, adding, randomScaling, isPhysics, numberOfParticles, range, size, scaling, gameTime);
            effectList.Add(effect);
        }

        /// <summary>
        /// Initialize a particle effect. This overload presumes an emitter that's not a point and has one texture
        /// </summary>
        /// <param name="Texture">The texture list to be used for the particle.</param>
        /// <param name="emitter">The source rectangle that provides an area where random particles will emit from.</param>
        /// <param name="newUpdate">The CustomUpdate delegate that is passed and run during each update if you want to supplement the physics engine. Nullable. Requires a function that has a ParticleEngine.ParticleEffect and a GameTime as parameters, returning void.</param>
        /// <param name="modColor">The color to shade the texture in.</param>
        /// <param name="rotation">This value indicates how quickly the given texture will rotate (0 for none). Between 0 and 1. The actual rotation is randomized with the value entered here as the maximum possible random value.</param>
        /// <param name="intensity">The speed at which the particles will move.</param>
        /// <param name="life">The total lifespan of a given particle. When a particle reaches its lifespan it's removed from the effect. Measured in milliseconds.</param>
        /// <param name="weight">The physical weight of the object. Gravity will affect the object based on the weight. A value of -1 will produce inverse gravity.</param>
        /// <param name="emissionDirection">The clockwise limit of your emission.</param>
        /// <param name="acceleration">A vector which can alter the direction of the particle depending on its age. Because the physics engine simply adds the calculated direction to the current position + acceleration, acceleration of any vector will anchor particles to more than one direction (good especially for providing a unit vector to encourage downward falling random particles, fore example)</param>
        /// <param name="adding">Is this effect something that continuously runs until asked to stop? If so, use StopEffect() to halt particle generation.</param>
        /// <param name="randomScaling">Determines if the given effect should scale each particle randomly with the Scaling value as its maximum scale (ideal for snowflakes?)</param>
        /// <param name="numberOfParticles">The number of particles to generate with each update if adding is true. Otherwise, the total number of particles to generate.</param>
        /// <param name="range">The range of direction, in degrees.</param>
        /// <param name="size">this is the size of the effect's motion in a vector (but construed as a float for a vector's distance). Values grater than 1 will multiply the natural size of the effect. Values below 1 will reduce the natural size of the effect. You'll usually want to go a few orders of magnitude higher.</param>
        /// <param name="scaling">The scale to draw the image.</param>
        /// <param name="gameTime">Current gametime to update age.</param>
        public void AddEffect(List<Texture2D> Texture, Rectangle emitter, CustomUpdate newUpdate, Color modColor, float rotation, float intensity, float life, float weight, int emissionDirection, Vector2 acceleration, bool adding, bool randomScaling, bool isPhysics, int numberOfParticles, int range, float size, float scaling, GameTime gameTime)
        {
            ParticleEffect effect = new ParticleEffect(game, Texture, emitter, newUpdate, modColor, rotation, intensity, life, weight, emissionDirection, acceleration, adding, randomScaling, isPhysics, numberOfParticles, range, size, scaling, gameTime);
            effectList.Add(effect);
        }

        /// <summary>
        /// Initialize a particle effect. This overload presumes an emitter that's not a point and has one texture
        /// </summary>
        /// <param name="Texture">The texture list to be used for the particle.</param>
        /// <param name="emitter">The source point from which all particles will come from.</param>
        /// <param name="newUpdate">The CustomUpdate delegate that is passed and run during each update if you want to supplement the physics engine. Nullable. Requires a function that has a ParticleEngine.ParticleEffect and a GameTime as parameters, returning void.</param>
        /// <param name="modColor">The color to shade the texture in.</param>
        /// <param name="rotation">This value indicates how quickly the given texture will rotate (0 for none). Between 0 and 1. The actual rotation is randomized with the value entered here as the maximum possible random value.</param>
        /// <param name="intensity">The speed at which the particles will move.</param>
        /// <param name="life">The total lifespan of a given particle. When a particle reaches its lifespan it's removed from the effect. Measured in milliseconds.</param>
        /// <param name="weight">The physical weight of the object. Gravity will affect the object based on the weight. A value of -1 will produce inverse gravity.</param>
        /// <param name="emissionDirection">The clockwise limit of your emission.</param>
        /// <param name="acceleration">A vector which can alter the direction of the particle depending on its age. Because the physics engine simply adds the calculated direction to the current position + acceleration, acceleration of any vector will anchor particles to more than one direction (good especially for providing a unit vector to encourage downward falling random particles, fore example)</param>
        /// <param name="adding">Is this effect something that continuously runs until asked to stop? If so, use StopEffect() to halt particle generation.</param>
        /// <param name="randomScaling">Determines if the given effect should scale each particle randomly with the Scaling value as its maximum scale (ideal for snowflakes?)</param>
        /// <param name="numberOfParticles">The number of particles to generate with each update if adding is true. Otherwise, the total number of particles to generate.</param>
        /// <param name="range">The range of direction, in degrees.</param>
        /// <param name="size">this is the size of the effect's motion in a vector (but construed as a float for a vector's distance). Values grater than 1 will multiply the natural size of the effect. Values below 1 will reduce the natural size of the effect. You'll usually want to go a few orders of magnitude higher.</param>
        /// <param name="scaling">The scale to draw the image.</param>
        /// <param name="gameTime">Current gametime to update age.</param>
        public void AddEffect(List<Texture2D> Texture, Vector2 emitter, CustomUpdate newUpdate, Color modColor, float rotation, float intensity, float life, float weight, int emissionDirection, Vector2 acceleration, bool adding, bool randomScaling, bool isPhysics, int numberOfParticles, int range, float size, float scaling, GameTime gameTime)
        {
            ParticleEffect effect = new ParticleEffect(game, Texture, emitter, newUpdate, modColor, rotation, intensity, life, weight, emissionDirection, acceleration, adding, randomScaling, isPhysics, numberOfParticles, range, size, scaling, gameTime);
            effectList.Add(effect);
        }

        /// <summary>
        /// Initialize a particle effect. This overload presumes an emitter that's not a point and has one texture
        /// </summary>
        /// <param name="Texture">The texture list to be used for the particle.</param>
        /// <param name="emitter">The reference to the source point from which all particles will come from. A reference ensures that particles react to the emitter's movement.</param>
        /// <param name="newUpdate">The CustomUpdate delegate that is passed and run during each update if you want to supplement the physics engine. Nullable. Requires a function that has a ParticleEngine.ParticleEffect and a GameTime as parameters, returning void.</param>
        /// <param name="modColor">The color to shade the texture in.</param>
        /// <param name="rotation">This value indicates how quickly the given texture will rotate (0 for none). Between 0 and 1. The actual rotation is randomized with the value entered here as the maximum possible random value.</param>
        /// <param name="intensity">The speed at which the particles will move.</param>
        /// <param name="life">The total lifespan of a given particle. When a particle reaches its lifespan it's removed from the effect. Measured in milliseconds.</param>
        /// <param name="weight">The physical weight of the object. Gravity will affect the object based on the weight. A value of -1 will produce inverse gravity.</param>
        /// <param name="emissionDirection">The clockwise limit of your emission.</param>
        /// <param name="acceleration">A vector which can alter the direction of the particle depending on its age. Because the physics engine simply adds the calculated direction to the current position + acceleration, acceleration of any vector will anchor particles to more than one direction (good especially for providing a unit vector to encourage downward falling random particles, fore example)</param>
        /// <param name="adding">Is this effect something that continuously runs until asked to stop? If so, use StopEffect() to halt particle generation.</param>
        /// <param name="isPhysics">Is this effect to be registered with the physics engine?</param>
        /// <param name="randomScaling">Determines if the given effect should scale each particle randomly with the Scaling value as its maximum scale (ideal for snowflakes?)</param>
        /// <param name="numberOfParticles">The number of particles to generate with each update if adding is true. Otherwise, the total number of particles to generate.</param>
        /// <param name="range">The range of direction, in degrees.</param>
        /// <param name="size">this is the size of the effect's motion in a vector (but construed as a float for a vector's distance). Values grater than 1 will multiply the natural size of the effect. Values below 1 will reduce the natural size of the effect. You'll usually want to go a few orders of magnitude higher.</param>
        /// <param name="scaling">The scale to draw the image.</param>
        /// <param name="gameTime">Current gametime to update age.</param>
        public void AddEffect(List<Texture2D> Texture, ref Vector2 emitter, CustomUpdate newUpdate, Color modColor, float rotation, float intensity, float life, float weight, int emissionDirection, Vector2 acceleration, bool adding, bool randomScaling, bool isPhysics, int numberOfParticles, int range, float size, float scaling, GameTime gameTime)
        {
            ParticleEffect effect = new ParticleEffect(game, Texture, emitter, newUpdate, modColor, rotation, intensity, life, weight, emissionDirection, acceleration, adding, randomScaling, isPhysics, numberOfParticles, range, size, scaling, gameTime);
            effectList.Add(effect);
        }

        /// <summary>
        /// Initialize a particle effect. This overload presumes an emitter that's not a point and has one texture
        /// </summary>
        /// <param name="Texture">The texture list to be used for the particle.</param>
        /// <param name="emitter">The reference to the source point from which all particles will come from. A reference ensures that particles react to the emitter's movement.</param>
        /// <param name="newUpdate">The CustomUpdate delegate that is passed and run during each update if you want to supplement the physics engine. Nullable. Requires a function that has a ParticleEngine.ParticleEffect and a GameTime as parameters, returning void.</param>
        /// <param name="modColor">The color to shade the texture in.</param>
        /// <param name="rotation">This value indicates how quickly the given texture will rotate (0 for none). Between 0 and 1. The actual rotation is randomized with the value entered here as the maximum possible random value.</param>
        /// <param name="intensity">The speed at which the particles will move.</param>
        /// <param name="life">The total lifespan of a given particle. When a particle reaches its lifespan it's removed from the effect. Measured in milliseconds.</param>
        /// <param name="weight">The physical weight of the object. Gravity will affect the object based on the weight. A value of -1 will produce inverse gravity.</param>
        /// <param name="emissionDirection">The clockwise limit of your emission.</param>
        /// <param name="acceleration">A vector which can alter the direction of the particle depending on its age. Because the physics engine simply adds the calculated direction to the current position + acceleration, acceleration of any vector will anchor particles to more than one direction (good especially for providing a unit vector to encourage downward falling random particles, fore example)</param>
        /// <param name="adding">Is this effect something that continuously runs until asked to stop? If so, use StopEffect() to halt particle generation.</param>
        /// <param name="isPhysics">Is this effect to be registered with the physics engine?</param>
        /// <param name="randomScaling">Determines if the given effect should scale each particle randomly with the Scaling value as its maximum scale (ideal for snowflakes?)</param>
        /// <param name="numberOfParticles">The number of particles to generate with each update if adding is true. Otherwise, the total number of particles to generate.</param>
        /// <param name="range">The range of direction, in degrees.</param>
        /// <param name="size">this is the size of the effect's motion in a vector (but construed as a float for a vector's distance). Values grater than 1 will multiply the natural size of the effect. Values below 1 will reduce the natural size of the effect. You'll usually want to go a few orders of magnitude higher.</param>
        /// <param name="scaling">The scale to draw the image.</param>
        /// <param name="gameTime">Current gametime to update age.</param>
        public void AddEffect(List<Texture2D> Texture, ref Rectangle emitter, CustomUpdate newUpdate, Color modColor, float rotation, float intensity, float life, float weight, int emissionDirection, Vector2 acceleration, bool adding, bool randomScaling, bool isPhysics, int numberOfParticles, int range, float size, float scaling, GameTime gameTime)
        {
            ParticleEffect effect = new ParticleEffect(game, Texture, emitter, newUpdate, modColor, rotation, intensity, life, weight, emissionDirection, acceleration, adding, randomScaling, isPhysics, numberOfParticles, range, size, scaling, gameTime);
            effectList.Add(effect);
        }
        public void StopEffects()
        {
            foreach (ParticleEffect effect in effectList)
                effect.StopEffect();
        }

        public void StopEffects(int offset)
        {
            effectList[offset].StopEffect();
        }

        /// <summary>
        /// The particle class that will generate a complete effect
        /// </summary>
        public class ParticleEffect
        {
            /// <summary>
            /// Initialize a particle effect. This overload presumes an emitter that's not a point and has one texture
            /// </summary>
            /// <param name="Texture">The texture to be used for the particle.</param>
            /// <param name="emitter">The source rectangle that provides an area where random particles will emit from.</param>
            /// <param name="newUpdate">The CustomUpdate delegate that is passed and run during each update if you want to supplement the physics engine. Nullable. Requires a function that has a ParticleEngine.ParticleEffect and a GameTime as parameters, returning void.</param>
            /// <param name="modColor">The color to shade the texture in.</param>
            /// <param name="intensity">The speed at which the particles will move.</param>
            /// <param name="life">The total lifespan of a given particle. When a particle reaches its lifespan it's removed from the effect.</param>
            /// <param name="weight">The physical weight of the object. Gravity will affect the object based on the weight. A value of -1 will produce inverse gravity.</param>
            /// <param name="emissionDirection">The clockwise limit of your emission.</param>
            /// <param name="acceleration">A vector which can alter the direction of the particle depending on its age</param>
            /// <param name="adding">Is this effect something that continuously runs until asked to stop? If so, use StopEffect() to halt particle generation.</param>
            /// <param name="numberOfParticles">The number of particles to generate with each update if adding is true. Otherwise, the total number of particles to generate.</param>
            /// <param name="range">The range of direction, in degrees.</param>
            /// <param name="size">this is the size of the effect's motion in a vector (but construed as a float for a vector's distance). Values grater than 1 will multiply the natural size of the effect. Values below 1 will reduce the natural size of the effect. You'll usually want to go a few orders of magnitude higher.</param>
            /// <param name="scaling">The scale to draw the image.</param>
            /// <param name="gameTime">Current gametime to update age.</param>
            public ParticleEffect(Game1 game, Texture2D Texture, Rectangle emitter, CustomUpdate newUpdate, Color modColor, float rotation, float intensity, float life, float weight, int emissionDirection, Vector2 acceleration, bool adding, bool randomScaling, bool isPhysics, int numberOfParticles, int range, float size, float scaling, GameTime gameTime)
            {
                this.game = game;

                TextureList = new List<Texture2D>();
                TextureList.Add(Texture);

                Emitter = new Rectangle();
                Emitter = emitter;

                // assign the delegate if there is one
                if(newUpdate != null)
                    customUpdate = new CustomUpdate(newUpdate);                     // make sure you add this usage into the Update code; call the delegate if not null; otherwise, allow the physics engine to handle it

                Intensity = intensity;
                Life = life;
                Weight = weight;
                EmissionDirection = emissionDirection;
                Acceleration = acceleration;
                isAdding = adding;
                isRandomScaling = randomScaling;
                isRegisteredPhysicsObject = isPhysics;
                NumberOfParticles = numberOfParticles;
                Rotation = rotation;
                EmissionRange = range;
                Size = size;
                ModColor = modColor;
                Scaling = scaling;

                for (int i = 0; i < numberOfParticles; i++)
                {
                    AddParticle(gameTime);
                }
            }

            /// <summary>
            /// Initialize a particle effect. This overload presumes an emitter that's a point and has one texture
            /// </summary>
            /// <param name="Texture"></param>
            /// <param name="emitter"></param>
            /// <param name="intensity"></param>
            /// <param name="life"></param>
            /// <param name="weight"></param>
            /// <param name="emissionDirection"></param>
            /// <param name="adding"></param>
            /// <param name="numberOfParticles"></param>
            /// <param name="size"></param>
            /// <param name="gameTime"></param>
            public ParticleEffect(Game1 game, Texture2D Texture, Vector2 emitter, CustomUpdate newUpdate, Color modColor, float rotation, float intensity, float life, float weight, int emissionDirection, Vector2 acceleration, bool adding, bool randomScaling, bool isPhysics, int numberOfParticles, int range, float size, float scaling, GameTime gameTime)
            {
                this.game = game;

                TextureList = new List<Texture2D>();
                TextureList.Add(Texture);

                EmitterPoint = new Vector2();
                EmitterPoint = emitter;

                // assign the delegate if there is one
                if (newUpdate != null)
                    customUpdate = new CustomUpdate(newUpdate);                     // make sure you add this usage into the Update code; call the delegate if not null; otherwise, allow the physics engine to handle it

                Intensity = intensity;
                Life = life;
                Weight = weight;
                EmissionDirection = emissionDirection;
                isAdding = adding;
                isRandomScaling = randomScaling;
                isRegisteredPhysicsObject = isPhysics;
                NumberOfParticles = numberOfParticles;
                Rotation = rotation;
                EmissionRange = range;
                Size = size;
                ModColor = modColor;
                Scaling = scaling;

                for (int i = 0; i < numberOfParticles; i++)
                {
                    AddParticle(gameTime);
                }
            }

            /// <summary>
            /// Initialize a particle effect. This overload presumes an emitter that's not a point and no more than one possible texture
            /// </summary>
            /// <param name="ListTexture"></param>
            /// <param name="emitter"></param>
            /// <param name="intensity"></param>
            /// <param name="life"></param>
            /// <param name="weight"></param>
            /// <param name="emissionDirection"></param>
            /// <param name="adding"></param>
            /// <param name="numberOfParticles"></param>
            /// <param name="size"></param>
            /// <param name="gameTime"></param>
            public ParticleEffect(Game1 game, List<Texture2D> ListTexture, Rectangle emitter, CustomUpdate newUpdate, Color modColor, float rotation, float intensity, float life, float weight, int emissionDirection, Vector2 acceleration, bool adding, bool randomScaling, bool isPhysics, int numberOfParticles, int range, float size, float scaling, GameTime gameTime)
            {
                this.game = game;

                TextureList = new List<Texture2D>();
                TextureList = ListTexture;

                Emitter = new Rectangle();
                Emitter = emitter;

                // assign the delegate if there is one
                if (newUpdate != null)
                    customUpdate = new CustomUpdate(newUpdate);                     // make sure you add this usage into the Update code; call the delegate if not null; otherwise, allow the physics engine to handle it

                Intensity = intensity;
                Life = life;
                Weight = weight;
                EmissionDirection = emissionDirection;
                isAdding = adding;
                isRandomScaling = randomScaling;
                isRegisteredPhysicsObject = isPhysics;
                NumberOfParticles = numberOfParticles;
                Rotation = rotation;
                EmissionRange = range;
                Size = size;
                ModColor = modColor;
                Scaling = scaling;

                for (int i = 0; i < numberOfParticles; i++)
                {
                    AddParticle(gameTime);
                }
            }

            /// <summary>
            /// Initialize a particle effect. This overload presumes an emitter that's a point and has more than one possible texture
            /// </summary>
            /// <param name="ListTexture"></param>
            /// <param name="emitter"></param>
            /// <param name="intensity"></param>
            /// <param name="life"></param>
            /// <param name="weight"></param>
            /// <param name="emissionDirection"></param>
            /// <param name="adding"></param>
            /// <param name="numberOfParticles"></param>
            /// <param name="size"></param>
            /// <param name="gameTime"></param>
            public ParticleEffect(Game1 game, List<Texture2D> ListTexture, Vector2 emitter, CustomUpdate newUpdate, Color modColor, float rotation, float intensity, float life, float weight, int emissionDirection, Vector2 acceleration, bool adding, bool randomScaling, bool isPhysics, int numberOfParticles, int range, float size, float scaling, GameTime gameTime)
            {
                this.game = game;

                TextureList = new List<Texture2D>();
                TextureList = ListTexture;

                EmitterPoint = new Vector2();
                EmitterPoint = emitter;

                // assign the delegate if there is one
                if (newUpdate != null)
                    customUpdate = new CustomUpdate(newUpdate);                     // make sure you add this usage into the Update code; call the delegate if not null; otherwise, allow the physics engine to handle it

                Intensity = intensity;
                Life = life;
                Weight = weight;
                EmissionDirection = emissionDirection;
                isAdding = adding;
                isRandomScaling = randomScaling;
                isRegisteredPhysicsObject = isPhysics;
                NumberOfParticles = numberOfParticles;
                Rotation = rotation;
                EmissionRange = range;
                Size = size;
                ModColor = modColor;
                Scaling = scaling;

                for (int i = 0; i < numberOfParticles; i++)
                {
                    AddParticle(gameTime);
                }
            }

            public ParticleEffect(Game1 game, List<Texture2D> ListTexture, ref Vector2 emitter, CustomUpdate newUpdate, Color modColor, float rotation, float intensity, float life, float weight, int emissionDirection, Vector2 acceleration, bool adding, bool randomScaling, bool isPhysics, int numberOfParticles, int range, float size, float scaling, GameTime gameTime)
            {
                this.game = game;

                TextureList = new List<Texture2D>();
                TextureList = ListTexture;

                EmitterPoint = new Vector2();
                EmitterPoint = emitter;

                // assign the delegate if there is one
                if (newUpdate != null)
                    customUpdate = new CustomUpdate(newUpdate);                     // make sure you add this usage into the Update code; call the delegate if not null; otherwise, allow the physics engine to handle it

                Intensity = intensity;
                Life = life;
                Weight = weight;
                EmissionDirection = emissionDirection;
                isAdding = adding;
                isRandomScaling = randomScaling;
                isRegisteredPhysicsObject = isPhysics;
                NumberOfParticles = numberOfParticles;
                Rotation = rotation;
                EmissionRange = range;
                Size = size;
                ModColor = modColor;
                Scaling = scaling;

                for (int i = 0; i < numberOfParticles; i++)
                {
                    AddParticle(gameTime);
                }
            }

            /// <summary>
            /// Initialize a particle effect. This overload presumes an emitter that's not a point and no more than one possible texture
            /// </summary>
            /// <param name="ListTexture"></param>
            /// <param name="emitter"></param>
            /// <param name="intensity"></param>
            /// <param name="life"></param>
            /// <param name="weight"></param>
            /// <param name="emissionDirection"></param>
            /// <param name="adding"></param>
            /// <param name="numberOfParticles"></param>
            /// <param name="size"></param>
            /// <param name="gameTime"></param>
            public ParticleEffect(Game1 game, List<Texture2D> ListTexture, ref Rectangle emitter, CustomUpdate newUpdate, Color modColor, float rotation, float intensity, float life, float weight, int emissionDirection, Vector2 acceleration, bool adding, bool randomScaling, bool isPhysics, int numberOfParticles, int range, float size, float scaling, GameTime gameTime)
            {
                this.game = game;

                TextureList = new List<Texture2D>();
                TextureList = ListTexture;

                Emitter = new Rectangle();
                Emitter = emitter;

                // assign the delegate if there is one
                if (newUpdate != null)
                    customUpdate = new CustomUpdate(newUpdate);                     // make sure you add this usage into the Update code; call the delegate if not null; otherwise, allow the physics engine to handle it

                Intensity = intensity;
                Life = life;
                Weight = weight;
                EmissionDirection = emissionDirection;
                isAdding = adding;
                isRandomScaling = randomScaling;
                isRegisteredPhysicsObject = isPhysics;
                NumberOfParticles = numberOfParticles;
                Rotation = rotation;
                EmissionRange = range;
                Size = size;
                ModColor = modColor;
                Scaling = scaling;

                for (int i = 0; i < numberOfParticles; i++)
                {
                    AddParticle(gameTime);
                }
            }

            /// <summary>
            /// called from the initiating scope to basically destroy a continuous effect
            /// </summary>
            public void StopEffect()
            {
                isAdding = false;
            }

            /// <summary>
            /// call this method if you want to add particles to the class during the update;
            /// should feed off a variable in the class that is initialized during class creation
            /// </summary>
            public void GenerateParticles(GameTime gameTime)                               // if isAdding is true, make sure this is called in the Update method
            {
                for (int i = 0; i < NumberOfParticles; i++)
                    AddParticle(gameTime);
            }

            public void AddParticle(GameTime gameTime)
            {
                ParticleStruct p = new ParticleStruct();

                p.BirthTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
                if (Emitter != null)
                    p.OrginalPosition = new Vector2(randomizer.Next(Emitter.Left, Emitter.Left + Emitter.Width), randomizer.Next(Emitter.Y, Emitter.Top + Emitter.Height));
                else
                    p.OrginalPosition = EmitterPoint;
                p.Position = p.OrginalPosition;
                p.Life = Life;
                if(isRandomScaling)
                    p.Scaling = (float)randomizer.NextDouble() * Scaling;
                else
                    p.Scaling = Scaling;              
                p.ModColor = ModColor;
                p.Weight = Weight;
                p.Rotation = (float)(randomizer.NextDouble() * Rotation);
                p.RotationFactor = Rotation;
                p.Rotation = 0;
                p.textureIndex = randomizer.Next(TextureList.Count);

                int rDestination = (int)(EmissionRange + EmissionDirection);
                float rDirection = (float)randomizer.Next(EmissionDirection, rDestination);
                if (rDirection >= 360)
                    rDirection -= 360;
                p.Direction = AngleToVector(MathHelper.ToRadians(rDirection));
                p.Direction *= new Vector2(Size, Size);
                p.Acceleration = Acceleration;
                p.isOnGround = false;
                p.isElemental = true;
                p.isModified = false;

                if (isRegisteredPhysicsObject)
                    game.physicsEngine.RegisterObject(p);                   // you may have a problem here; I think structs deep copy if assigned to a new struct (which I think implicitly happens when added to a list?)

                particleList.Add(p);
            }

            Game1 game;
            Random randomizer = new Random();
            public List<Texture2D> TextureList;                                                    // during the draw method, make sure that this implements a random texture from this list; otherwise use the single Texture2D if filled
            public Rectangle Emitter;
            
            /// <summary>
            /// a point that indicates an emission point (i.e., all OriginalPositions will be this point)
            /// </summary>
            Vector2 EmitterPoint;
            /// <summary>
            /// one end of the total range of directions possible
            /// </summary>
            int EmissionDirection;
            Vector2 Acceleration;
            Color ModColor;
            float Intensity;
            float Life;    // formerly MaxAge
            float Weight;
            // used to not show the beginning or the end of the effect if they're not wanted
            float VisibilityRange;
            float Rotation;

            /// <summary>
            /// is this particle effect constantly adding particles?
            /// </summary>
            public bool isAdding;
            public bool isRandomScaling;
            public bool isRegisteredPhysicsObject;
            public int NumberOfParticles;
            int EmissionRange;                                              // 360 degrees would mean all directions
            float Size;
            float Scaling;

            public CustomUpdate customUpdate;                                      // we want to be able to add a custom function to update particles if we're not satisfied with the flexibility of the physics engine

            public List<ParticleStruct> particleList = new List<ParticleStruct>();

            /// <summary>
            /// returns the vector for a given angle
            /// </summary>
            /// <param name="angle"></param>
            /// <returns></returns>
            public Vector2 AngleToVector(float angle)
            {
                return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
            }

            /// <summary>
            /// returns an angle (in radians or degrees?) for the given vector
            /// </summary>
            /// <param name="vector"></param>
            /// <returns></returns>
            public float VectorToAngle(Vector2 vector)
            {
                return (float)Math.Atan2(vector.X, -vector.Y);
            }
        }

        public class ParticleStruct : IPhysicalObject
        {
            public ParticleStruct() { }
            public float BirthTime { get; set; }
            public Vector2 OrginalPosition { get; set; }
            public Vector2 Acceleration { get; set; }
            public Vector2 Direction { get; set; }
            public Vector2 Position { get; set; }
            public float Scaling { get; set; }
            public Color ModColor { get; set; }
            public float Rotation { get; set; }
            public float RotationFactor { get; set; }
            public float Life { get; set; }    // formerly MaxAge
            public float Weight { get; set; }
            public int textureIndex { get; set; }
            public bool isOnGround { get; set; }
            public bool isElemental { get; set; }

            /// <summary>
            /// used to modify the given particle after the initial setup; set to false initially, so you can set to true later
            /// </summary>
            public bool isModified { get; set; }
        }

        public void AddWaterBurst(Vector2 waterPos, int numberOfParticles, float size, float maxAge, GameTime gameTime)
        {
            for (int i = 0; i < numberOfParticles; i++)
                AddBurstWaterParticle(waterPos, size, maxAge, gameTime);
        }

        private void AddBurstWaterParticle(Vector2 waterPos, float waterSize, float maxAge, GameTime gameTime)
        {
            //WaterData particle = new WaterData();

            //particle.OrginalPosition = waterPos;
            //particle.Position = particle.OrginalPosition;

            //particle.BirthTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            //particle.MaxAge = maxAge;
            //particle.Scaling = 0.25f;
            //particle.ModColor = Color.White;

            float particleDistance = (float)randomizer.NextDouble() * waterSize;            // 0.x * the size of the explosion yields max of desired size of explosion and min of 0
            Vector2 displacement = new Vector2(particleDistance, 0);                        // creates the unit vector (pretty much the x-axis) based on the distance we'll be going
            float angle = MathHelper.ToRadians(randomizer.Next(360));                       // generate a random angle; this will pretty much the direction the particle will fly
            displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(angle));  // now take the vector that indicates the distance it will go and point it in the random direction from above

            //particle.Direction = displacement * 2.0f;
            //particle.Accelaration = -particle.Direction;                                    // acceleration is the inverse of the direction; why? so that the particle may start in one direction but as time passes the acceleration will influence it more (or less)
                                                                                            // in the explosion example, we see that the acceleration is what helps the explosion rotate on the outer edges of the explosion (furthest away from the origin)
            //currentTexture = water;                                                         // I suppose this contributes to the random and explosive nature of ...an explosion
            //particleSystem = ParticleSystem.explosion;                                      // in most cases, however, the acceleration should probably align with the direction so that the particle doesn't change course (unless, of couse, you want it to)
                                                                                            // to do that, just set acceleration to 0,0
            //particleList.Add(particle);
        }


        public ParticleEngine(Game1 game)
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
            // TODO: Add your initialization code here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
                    if (effectList.Count > 0)
                    {
                        double now = gameTime.TotalGameTime.TotalMilliseconds;
                        for(int i = 0; i < effectList.Count; i++)
                        {
                            if (effectList[i].customUpdate != null)                            // if an alternative/additional method was used to supplement the physics engine, use the custom update method
                                effectList[i].customUpdate(effectList[i], gameTime);
                            for(int j = 0; j < effectList[i].particleList.Count; j++)   // remove all particles that are being removed from the physics engine
                            {
                                if (now - effectList[i].particleList[j].BirthTime > effectList[i].particleList[j].Life)
                                {
                                    effectList[i].particleList.RemoveAt(j);
                                }
                            }
                            if (effectList[i].isAdding)                                        // add particles if the effect calls for it
                                effectList[i].GenerateParticles(gameTime);
                            if (effectList[i].particleList.Count == 0)                         // remove all effects that have fizzled out
                                effectList.RemoveAt(i);
                        }
                    }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
                    spriteBatch.Begin(SpriteBlendMode.Additive, SpriteSortMode.Deferred, SaveStateMode.None);
                    foreach(ParticleEffect effect in effectList)
                    {
                        foreach (ParticleStruct particle in effect.particleList)
                        {
                            Texture2D texture = effect.TextureList[particle.textureIndex];
                            spriteBatch.Draw(texture, particle.Position, null, particle.ModColor, particle.Rotation, new Vector2(texture.Width / 2, texture.Height / 2), particle.Scaling, SpriteEffects.None, 1);
                        }
                    }
                    spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}