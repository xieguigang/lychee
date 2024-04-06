namespace utils {

    export function removeElement(node: HTMLElement) {
        node.parentNode.removeChild(node);
    }

    export function getObsoletes(): HTMLElement[] {
        return $ts.select(".delete").ToArray();
    }
}

