using JSONAPI.Core;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Service to provide formatting of links
    /// </summary>
    public interface ILinkConventions
    {
        /// <summary>
        /// Builds a relationship link for the given model property
        /// </summary>
        /// <param name="relationshipOwner">The resource that this relationship belongs to.</param>
        /// <param name="modelManager">The model manage to use to lookup information about the relationship owner's type</param>
        /// <param name="property"></param>
        /// <returns></returns>
        ILink GetRelationshipLink<TResource>(TResource relationshipOwner, IModelManager modelManager, RelationshipModelProperty property);

        /// <summary>
        /// Builds a related resource link for the given model property
        /// </summary>
        /// <param name="relationshipOwner">The resource that this relationship belongs to.</param>
        /// <param name="modelManager">The model manage to use to lookup information about the relationship owner's type</param>
        /// <param name="property"></param>
        /// <returns></returns>
        ILink GetRelatedResourceLink<TResource>(TResource relationshipOwner, IModelManager modelManager, RelationshipModelProperty property);
    }
}
