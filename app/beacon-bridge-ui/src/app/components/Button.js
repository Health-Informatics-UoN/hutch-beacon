export default function Button({icon, text, onClick}) {
  return (
    <button className="w-24 bg-blue-950 px-2" onClick={onClick}>
      <span className={"flex items-center space-x-1"}>
          {icon}
          <p className="grow">{text}</p>
        </span>
    </button>
  )
}
