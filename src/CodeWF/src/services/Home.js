import request from "@/utils/request";

export async function getHomeLinks() {
  return request({
    url: "/api/link",
    method: "get",
  });
}

export async function getHomeTool() {
  return request({
    url: "/api/home",
    method: "get",
  });
}

// export async function getHomeLinks() {
//     return request({
//         url: '/api/home',
//         method: 'get',
//     });
// }

//
