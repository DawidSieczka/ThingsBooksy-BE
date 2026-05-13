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

import { ThingsBooksySharedInfrastructureModulesModuleInfo } from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

export class Modules<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags ThingsBooksy.Bootstrapper
   * @name ModulesList
   * @request GET:/modules
   * @secure
   */
  modulesList = (params: RequestParams = {}) =>
    this.request<ThingsBooksySharedInfrastructureModulesModuleInfo[], any>({
      path: `/modules`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
