/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

/** @format int32 */
export enum ThingsBooksyModulesResourcesCoreDomainPropertyDataType {
  Value0 = 0,
  Value1 = 1,
  Value2 = 2,
}

export interface ThingsBooksyModulesManagementGroupsApiRequestsAddGroupMemberRequest {
  email?: string | null;
}

export interface ThingsBooksyModulesManagementGroupsApiRequestsCreateManagementGroupRequest {
  name?: string | null;
  description?: string | null;
}

export interface ThingsBooksyModulesManagementGroupsApiRequestsUpdateManagementGroupRequest {
  name?: string | null;
  description?: string | null;
}

export interface ThingsBooksyModulesManagementGroupsCoreFeaturesGetManagementGroupModelsResultsManagementGroupMemberResult {
  /** @format uuid */
  userId?: string;
  /** @format date-time */
  joinedAt?: string;
}

export interface ThingsBooksyModulesManagementGroupsCoreFeaturesGetManagementGroupGetManagementGroupQueryResult {
  /** @format uuid */
  id?: string;
  name?: string | null;
  description?: string | null;
  /** @format uuid */
  ownerId?: string;
  /** @format date-time */
  createdAt?: string;
  /** @format int32 */
  memberCount?: number;
  members?:
    | ThingsBooksyModulesManagementGroupsCoreFeaturesGetManagementGroupModelsResultsManagementGroupMemberResult[]
    | null;
}

export interface ThingsBooksyModulesManagementGroupsCoreFeaturesGetGroupMembersGroupMemberDto {
  /** @format uuid */
  memberId?: string;
  /** @format uuid */
  userId?: string;
  email?: string | null;
  /** @format date-time */
  joinedAt?: string;
  isOwner?: boolean;
}

export interface ThingsBooksyModulesManagementGroupsCoreFeaturesGetGroupMembersGetGroupMembersQueryResult {
  items?: ThingsBooksyModulesManagementGroupsCoreFeaturesGetGroupMembersGroupMemberDto[] | null;
  /** @format uuid */
  nextCursor?: string | null;
}

export interface ThingsBooksyModulesManagementGroupsCoreFeaturesIsGroupNameAvailableIsGroupNameAvailableQueryResult {
  available?: boolean;
}

export interface ThingsBooksyModulesResourcesApiRequestsCreateResourceInstanceRequest {
  /** @format uuid */
  resourceTypeId?: string;
  name?: string | null;
  description?: string | null;
  propertyValues?:
    | ThingsBooksyModulesResourcesApiRequestsPropertyValueInputDto[]
    | null;
}

export interface ThingsBooksyModulesResourcesApiRequestsCreateResourceTypeRequest {
  /** @format uuid */
  groupId?: string;
  name?: string | null;
  description?: string | null;
  propertyDefinitions?:
    | ThingsBooksyModulesResourcesApiRequestsPropertyDefinitionInputDto[]
    | null;
}

export interface ThingsBooksyModulesResourcesApiRequestsPropertyDefinitionInputDto {
  name?: string | null;
  dataType?: ThingsBooksyModulesResourcesCoreDomainPropertyDataType;
  isRequired?: boolean;
}

export interface ThingsBooksyModulesResourcesApiRequestsPropertyDefinitionUpdateInputDto {
  /** @format uuid */
  id?: string | null;
  name?: string | null;
  dataType?: ThingsBooksyModulesResourcesCoreDomainPropertyDataType;
  isRequired?: boolean;
}

export interface ThingsBooksyModulesResourcesApiRequestsPropertyValueInputDto {
  /** @format uuid */
  propertyDefinitionId?: string;
  value?: string | null;
}

export interface ThingsBooksyModulesResourcesApiRequestsUpdatePropertyValueDto {
  /** @format uuid */
  propertyDefinitionId?: string;
  value?: string | null;
}

export interface ThingsBooksyModulesResourcesApiRequestsUpdateResourceInstanceRequest {
  name?: string | null;
  description?: string | null;
  propertyValues?:
    | ThingsBooksyModulesResourcesApiRequestsUpdatePropertyValueDto[]
    | null;
}

export interface ThingsBooksyModulesResourcesApiRequestsUpdateResourceTypeRequest {
  name?: string | null;
  description?: string | null;
  propertyDefinitions?:
    | ThingsBooksyModulesResourcesApiRequestsPropertyDefinitionUpdateInputDto[]
    | null;
}

export interface ThingsBooksyModulesResourcesCoreFeaturesGetResourceInstanceModelsPropertyValueResult {
  /** @format uuid */
  propertyDefinitionId?: string;
  propertyName?: string | null;
  dataType?: string | null;
  value?: string | null;
}

export interface ThingsBooksyModulesResourcesCoreFeaturesGetResourceInstancesResourceInstanceRowDto {
  /** @format uuid */
  id?: string;
  /** @format uuid */
  resourceTypeId?: string;
  /** @format uuid */
  groupId?: string;
  name?: string | null;
  description?: string | null;
  /** @format uuid */
  ownerId?: string;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  deletedAt?: string | null;
  propertyValues?:
    | ThingsBooksyModulesResourcesCoreFeaturesGetResourceInstanceModelsPropertyValueResult[]
    | null;
}

export interface ThingsBooksyModulesResourcesCoreFeaturesGetResourceInstancesGetResourceInstancesQueryResult {
  items?:
    | ThingsBooksyModulesResourcesCoreFeaturesGetResourceInstancesResourceInstanceRowDto[]
    | null;
  /** @format uuid */
  nextCursor?: string | null;
}

export interface ThingsBooksyModulesUsersApiRequestsSignInRequest {
  email?: string | null;
  password?: string | null;
}

export interface ThingsBooksyModulesUsersApiRequestsSignUpRequest {
  email?: string | null;
  password?: string | null;
  jobTitle?: string | null;
  role?: string | null;
}

export interface ThingsBooksySharedAbstractionsAppInfo {
  name?: string | null;
  version?: string | null;
}

export interface ThingsBooksySharedInfrastructureModulesModuleInfo {
  name?: string | null;
  policies?: string[] | null;
}
