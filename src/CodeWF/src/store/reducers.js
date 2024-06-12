const defaultState = {
  counter: 0,
  AllLinks: [],
  correlationTool: {
    tools: [],
    blogPosts: [],
    base: {},
  }
};

export default (state = defaultState, action) => {
  if (action.type === "CHANGE_ALLLINKS") {
    return { ...state, AllLinks: action.value };
  }

  if (action.type === "CHANGE_CORRELATIONTOOL") {
    console.log(action.value,'action.value');
    return { ...state, correlationTool: action.value };

  }

  return state;
};
