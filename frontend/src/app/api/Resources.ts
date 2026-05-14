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

import {
  ThingsBooksyModulesResourcesApiRequestsCreateResourceInstanceRequest,
  ThingsBooksyModulesResourcesApiRequestsCreateResourceTypeRequest,
  ThingsBooksyModulesResourcesApiRequestsUpdateResourceInstanceRequest,
  ThingsBooksyModulesResourcesApiRequestsUpdateResourceTypeRequest,
  ThingsBooksyModulesResourcesCoreFeaturesGetResourceInstancesGetResourceInstancesQueryResult,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class Resources<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags Resources
   * @name CreateResourceType
   * @request POST:/resources/types
   * @secure
   */
  createResourceType = (
    data: ThingsBooksyModulesResourcesApiRequestsCreateResourceTypeRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/resources/types`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Resources
   * @name GetResourceTypes
   * @request GET:/resources/types
   * @secure
   */
  getResourceTypes = (
    query: {
      /** @format uuid */
      groupId: string;
    },
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/resources/types`,
      method: "GET",
      query: query,
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags Resources
   * @name UpdateResourceType
   * @request PUT:/resources/types/{id}
   * @secure
   */
  updateResourceType = (
    id: string,
    data: ThingsBooksyModulesResourcesApiRequestsUpdateResourceTypeRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/resources/types/${id}`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Resources
   * @name DeleteResourceType
   * @request DELETE:/resources/types/{id}
   * @secure
   */
  deleteResourceType = (id: string, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/resources/types/${id}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags Resources
   * @name GetResourceType
   * @request GET:/resources/types/{id}
   * @secure
   */
  getResourceType = (id: string, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/resources/types/${id}`,
      method: "GET",
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags Resources
   * @name CreateResourceInstance
   * @request POST:/resources/instances
   * @secure
   */
  createResourceInstance = (
    data: ThingsBooksyModulesResourcesApiRequestsCreateResourceInstanceRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/resources/instances`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @summary Returns a cursor-paginated list of resource instances. Use afterId + take for forward-only infinite scroll.
   *
   * @tags Resources
   * @name GetResourceInstances
   * @request GET:/resources/instances
   * @secure
   */
  getResourceInstances = (
    query?: {
      /** @format uuid */
      resourceTypeId?: string | null;
      /** @format uuid */
      groupId?: string | null;
      includeDeleted?: boolean | null;
      /** @format uuid */
      afterId?: string | null;
      /** @format int32 */
      take?: number | null;
    },
    params: RequestParams = {},
  ) =>
    this.request<
      ThingsBooksyModulesResourcesCoreFeaturesGetResourceInstancesGetResourceInstancesQueryResult,
      any
    >({
      path: `/resources/instances`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Resources
   * @name GetResourceInstance
   * @request GET:/resources/instances/{id}
   * @secure
   */
  getResourceInstance = (id: string, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/resources/instances/${id}`,
      method: "GET",
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags Resources
   * @name UpdateResourceInstance
   * @request PUT:/resources/instances/{id}
   * @secure
   */
  updateResourceInstance = (
    id: string,
    data: ThingsBooksyModulesResourcesApiRequestsUpdateResourceInstanceRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/resources/instances/${id}`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Resources
   * @name DeleteResourceInstance
   * @request DELETE:/resources/instances/{id}
   * @secure
   */
  deleteResourceInstance = (id: string, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/resources/instances/${id}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
}
