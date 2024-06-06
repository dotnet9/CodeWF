
const defaultState = {
    counter: 0,
    AllLinks: []
}


export default (state = defaultState, action) => {

    if (action.type === 'CHANGE_ALLLINKS') {
        return { ...state, AllLinks: action.value }
    }

    return state
}