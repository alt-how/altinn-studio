using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Authorization.ABAC.Utils;

namespace Altinn.Authorization.ABAC.Xacml
{
    /// <summary>
    /// From XACML Specification https://docs.oasis-open.org/xacml/3.0/xacml-3.0-core-spec-os-en.html#_Toc325047126
    /// The <Rule/> element SHALL define the individual rules in the policy.  The main components of this element are the <Target/>, <Condition/>,
    /// <ObligationExpressions/>  and <AdviceExpressions/>  elements and the Effect attribute.
    /// A<Rule/> element may be evaluated, in which case the evaluation procedure defined in Section 7.10 SHALL be used.
    ///
    /// The <Rule/> element is of RuleType complex type.
    /// The<Rule/> element contains the following attributes and elements:
    ///
    /// RuleId[Required]
    /// A string identifying this rule.
    /// 
    /// Effect[Required]
    /// Rule effect.The value of this attribute is either “Permit” or “Deny”.
    ///
    /// <Description/> [Optional]
    /// A free-form description of the rule.
    ///
    /// <Target/> [Optional]
    /// Identifies the set of decision requests that the <Rule/> element is intended to evaluate.If this element is omitted, then the target for the<Rule/> SHALL be defined by
    /// the <Target/> element of the enclosing <Policy/> element.See Section 7.7 for details.
    ///
    /// <Condition/> [Optional]
    /// A predicate that MUST be satisfied for the rule to be assigned its Effect value.
    ///
    /// <ObligationExpressions/> [Optional]
    /// A conjunctive sequence of obligation expressions which MUST be evaluated into obligations byt the PDP.The corresponsding obligations MUST be fulfilled by the
    /// PEP in conjunction with the authorization decision.
    /// See Section 7.18 for a description of how the set of obligations to be returned by the PDP SHALL be determined.See section 7.2 about enforcement of obligations.
    ///
    /// <AdviceExpressions/> [Optional]
    /// A conjunctive sequence of advice expressions which MUST evaluated into advice by the PDP. The corresponding advice provide supplementary information to the PEP in conjunction with the
    /// authorization decision.See Section 7.18 for a description of how the set of advice to be returned by the PDP SHALL be determined.
    /// </summary>
    public class XacmlRule
    {
        private readonly ICollection<XacmlObligationExpression> obligations = new Collection<XacmlObligationExpression>();
        private readonly ICollection<XacmlAdviceExpression> advices = new Collection<XacmlAdviceExpression>();
        private string ruleId;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlRule"/> class.
        /// </summary>
        /// <param name="ruleId">The rule identifier.</param>
        /// <param name="effect">The rule effect.</param>
        public XacmlRule(string ruleId, XacmlEffectType effect)
        {
            Guard.ArgumentNotNull(ruleId, nameof(ruleId));
            this.ruleId = ruleId;
            this.Effect = effect;
        }

        /// <summary>
        /// Gets or sets A free-form description of the rule.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Get or sets Target that Identifies the set of decision requests that the <Rule/> element is intended to evaluate.If this element is omitted,
        /// then the target for the<Rule/> SHALL be defined by the <Target/> element of the enclosing <Policy/> element.See Section 7.7 for details.
        /// </summary>
        public XacmlTarget Target { get; set; }

        /// <summary>
        ///  A predicate that MUST be satisfied for the rule to be assigned its Effect value.
        /// </summary>
        public XacmlExpression Condition { get; set; }

        /// <summary>
        ///  Gets or set A string identifying this rule.
        /// </summary>
        public string RuleId
        {
            get
            {
                return this.ruleId;
            }

            set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                this.ruleId = value;
            }
        }

        /// <summary>
        /// Gets or set Rule effect.The value of this attribute is either “Permit” or “Deny”.
        /// </summary>
        public XacmlEffectType Effect { get; set; }

        /// <summary>
        /// A conjunctive sequence of obligation expressions which MUST be evaluated into obligations byt the PDP.The corresponsding obligations MUST be fulfilled by the PEP in
        /// conjunction with the authorization decision.
        /// See Section 7.18 for a description of how the set of obligations to be returned by the PDP SHALL be determined.See section 7.2 about enforcement of obligations. 
        /// </summary>
        public ICollection<XacmlObligationExpression> Obligations
        {
            get
            {
                return this.obligations;
            }
        }

        /// <summary>
        /// A conjunctive sequence of advice expressions which MUST evaluated into advice by the PDP. The corresponding advice provide supplementary information to the PEP in conjunction with the
        /// authorization decision.See Section 7.18 for a description of how the set of advice to be returned by the PDP SHALL be determined.
        /// </summary>
        public ICollection<XacmlAdviceExpression> Advices
        {
            get
            {
                return this.advices;
            }
        }

        /// <summary>
        /// Validates if a rule matches 
        /// </summary>
        /// <param name="request">The XACML Context request</param>
        /// <returns></returns>
        public bool IsTargetResourceMatch(XacmlContextRequest request)
        {
            Dictionary<string, XacmlAttribute> requestResources = GetResources(request);

            bool isMatch = false;

            bool resourcesFound = false;

            foreach (XacmlAnyOf anyOf in Target.AnyOf)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    bool allResourcesMatched = true;

                    foreach (XacmlMatch xacmlMatch in allOf.Matches)
                    {
                        if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Resource))
                        {
                            resourcesFound = true;

                            if (requestResources.ContainsKey(xacmlMatch.AttributeDesignator.AttributeId.OriginalString))
                            {
                                foreach (XacmlAttributeValue attValue in requestResources[xacmlMatch.AttributeDesignator.AttributeId.OriginalString].AttributeValues)
                                {
                                    if (!xacmlMatch.IsMatch(attValue))
                                    {
                                        allResourcesMatched = false;
                                    }
                                }
                            }
                            else
                            {
                                allResourcesMatched = false;
                            }
                        }
                    }

                    if (allResourcesMatched && resourcesFound)
                    {
                        // All allOff matches for resources in a anyOff did match.
                        isMatch = true;
                    }
                }
            }

            return isMatch;
        }

        /// <summary>
        /// Verify that rule matches 
        /// </summary>
        /// <param name="request">The context request</param>
        /// <returns></returns>
        public bool IsTargetActionMatch(XacmlContextRequest request)
        {
            Dictionary<string, XacmlAttribute> requestActions = GetActions(request);

            if (requestActions.Count != 1)
            {
                return false;
            }

            bool isMatch = false;

            bool actionsFound = false;

            foreach (XacmlAnyOf anyOf in Target.AnyOf)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    bool allActionsMatched = true;

                    foreach (XacmlMatch xacmlMatch in allOf.Matches)
                    {
                        if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Resource))
                        {
                            actionsFound = true;

                            if (requestActions.ContainsKey(xacmlMatch.AttributeDesignator.AttributeId.OriginalString))
                            {
                                foreach (XacmlAttributeValue attValue in requestActions[xacmlMatch.AttributeDesignator.AttributeId.OriginalString].AttributeValues)
                                {
                                    if (!xacmlMatch.IsMatch(attValue))
                                    {
                                        allActionsMatched = false;
                                    }
                                }
                            }
                            else
                            {
                                allActionsMatched = false;
                            }
                        }
                    }

                    if (allActionsMatched && actionsFound)
                    {
                        // All allOff matches for actions in a anyOff did match.
                        isMatch = true;
                    }
                }
            }

            return isMatch;
        }

        /// <summary>
        /// Authorized a claims principal based on the subject rules and
        /// </summary>
        /// <param name="claimsPrincipal">The principal</param>
        /// <returns></returns>
        public XacmlContextDecision AuthorizeSubject(ClaimsPrincipal claimsPrincipal)
        {
            XacmlContextDecision decision = XacmlContextDecision.NotApplicable;

            foreach (XacmlAnyOf anyOf in Target.AnyOf)
            {
                foreach (XacmlAllOf allOf in anyOf.AllOf)
                {
                    bool allSubjectAttributesMatched = true;

                    foreach (XacmlMatch xacmlMatch in allOf.Matches)
                    {
                        bool matched = false;
                        if (xacmlMatch.AttributeDesignator.Category.Equals(XacmlConstants.MatchAttributeCategory.Subject))
                        {
                            // Get all the claims that matches the rule
                            IEnumerable<Claim> possibleMatchingClaims = claimsPrincipal.Claims.Where(c => c.Type.Equals(xacmlMatch.AttributeDesignator.AttributeId.OriginalString));
                            foreach (Claim claim in possibleMatchingClaims)
                            {
                                if (xacmlMatch.IsMatch(claim.Value))
                                {
                                    matched = true;
                                    break;
                                }
                            }
                        }
                        
                        if (!matched)
                        {
                            allSubjectAttributesMatched = false;
                        }
                    }

                    if (allSubjectAttributesMatched)
                    {
                        if (Effect.Equals(XacmlEffectType.Deny))
                        {
                            decision = XacmlContextDecision.Deny;
                        }
                        else
                        {
                            decision = XacmlContextDecision.Permit;
                        }
                    }
                }
            }

            return decision;
        }

        private Dictionary<string, XacmlAttribute> GetActions(XacmlContextRequest request)
        {
            Dictionary<string, XacmlAttribute> resourceAttributes = new Dictionary<string, XacmlAttribute>();
            foreach (XacmlContextAttributes attributes in request.Attributes)
            {
                if (attributes.Category.Equals(XacmlConstants.MatchAttributeCategory.Action))
                {
                    foreach (XacmlAttribute attribute in attributes.Attributes)
                    {
                        resourceAttributes.Add(attribute.AttributeId.OriginalString, attribute);
                    }
                }
            }

            return resourceAttributes;
        }

        private Dictionary<string, XacmlAttribute> GetResources(XacmlContextRequest request)
        {
            Dictionary<string, XacmlAttribute> resourceAttributes = new Dictionary<string, XacmlAttribute>();
            foreach (XacmlContextAttributes attributes in request.Attributes)
            {
                if (attributes.Category.Equals(XacmlConstants.MatchAttributeCategory.Resource))
                {
                    foreach (XacmlAttribute attribute in attributes.Attributes)
                    {
                        resourceAttributes.Add(attribute.AttributeId.OriginalString, attribute);
                    }
                }
            }

            return resourceAttributes;
        }
    }
}
