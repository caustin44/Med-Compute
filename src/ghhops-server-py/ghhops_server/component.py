class HopsComponent:
    """Hops Component"""

    def __init__(
        self,
        uri,
        name,
        nickname,
        desc,
        cat,
        subcat,
        icon,
        inputs,
        outputs,
        handler,
    ):
        self.uri = uri
        # TODO: customize solve uri?
        self.solve_uri = uri
        self.name = name
        self.nickname = nickname
        self.description = desc
        self.category = cat
        self.subcategory = subcat
        self.icon = icon
        self.inputs = inputs or []
        self.outputs = outputs or []
        self.handler = handler

    def __str__(self):
        return repr(self)

    def __repr__(self):
        inputs_repr = ",".join([x.name for x in self.inputs])
        outputs_repr = ",".join([x.name for x in self.outputs])
        return (
            f"<{self.__class__.__name__} "
            f"{self.uri} "
            f"[{inputs_repr} -> {self.name} -> {outputs_repr}] >"
        )

    def encode(self):
        """Serializer"""
        metadata = {
            "Uri": self.solve_uri,
            "Name": self.description,
            "Nickname": self.description,
            "Description": self.description,
            "Category": self.description,
            "Subcategory": self.description,
            "Inputs": self.inputs,
            "Outputs": self.outputs,
        }
        if self.icon:
            metadata["Icon"] = self.icon
        return metadata
