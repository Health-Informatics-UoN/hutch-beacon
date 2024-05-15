import Image from "next/image";

export default function Header() {
  return (
    <div>
      <span className={"flex items-baseline space-x-14"}>
        <Image src={"/UoN-Nottingham-Blue-white-text-logo-RGB-300x112.png"} width={300} height={112} />
        <h1 className={"text-6xl"}>BEACON</h1>
      </span>
    </div>
  )
}
