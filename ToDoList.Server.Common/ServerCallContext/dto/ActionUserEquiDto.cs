////-----------------------------------------------------------------------
//// <copyright file="ActionUserEquiDto.cs" company="">
//// Copyright (c) . All rights reserved.
//// </copyright>
////-----------------------------------------------------------------------

//namespace Server.FrontEndService.UserSecurity.Contracts.Dtos
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Runtime.Serialization;
//    using Common;
//    using Server.FrontEndService.User.Contracts.Dtos;

//    /// <summary>
//    /// User Group Equi Data Transfer Object
//    /// </summary>
//    [Serializable]
//    public class ActionUserEquiDto : BaseDto
//    {
//        /// <summary>
//        /// Gets or sets the Action Id
//        /// </summary>
//        [DataMember(Name = "ActionId", Order = 1)]
//        public ExternalObjId ActionId { get; set; }

//        /// <summary>
//        /// Gets or sets the Action Identifier
//        /// </summary>
//        [DataMember(Name = "ActionIdentifier", Order = 2)]
//        public string ActionIdentifier { get; set; }

//        /// <summary>
//        /// Gets or sets the Equipments
//        /// </summary>
//        [DataMember(Name = "Equipments", Order = 3)]
//        public IEnumerable<EquipmentDto> Equipments { get; set; }

//        /// <summary>
//        /// Gets or sets the Uegrs
//        /// </summary>
//        [DataMember(Name = "Uegrs", Order = 4)]
//        public IEnumerable<UserEquiGroupBaseDto> Uegrs { get; set; }
//    }
//}